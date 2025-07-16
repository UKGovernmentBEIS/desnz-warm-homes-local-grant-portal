using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using WhlgPortalWebsite.BusinessLogic;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Tests.Builders;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Controllers;

[TestFixture]
public class HomeFileControllerTests
{
    private HomeController underTest;
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private Mock<IFileRetrievalService> mockFileRetrievalService;
    private Mock<IWebHostEnvironment> mockWebHostEnvironment;

    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockFileRetrievalService = new Mock<IFileRetrievalService>();
        mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        var userDataStore = new UserService(mockDataAccessProvider.Object);

        underTest = new HomeController(userDataStore, mockFileRetrievalService.Object, mockWebHostEnvironment.Object);
        underTest.ControllerContext.HttpContext = new HttpContextBuilder(EmailAddress).Build();
        underTest.Url = new Mock<IUrlHelper>().Object;
    }

    [Test]
    public async Task Index_WhenUserViewsForTheFirstTime_SetsLoggedInFlag()
    {
        // Arrange
        var fileData = new PaginatedFileData
        {
            CurrentPage = 1,
            MaximumPage = 1,
            FileData = new List<LocalAuthorityFileData>()
            {
                new("114", 1, 2023, new DateTime(2023, 1, 31), null)
            }
        };
        var user = new UserBuilder(EmailAddress)
            .WithHasLoggedIn(false)
            .WithLocalAuthorities(new List<LocalAuthority>
            {
                new()
                {
                    Id = 1,
                    CustodianCode = "114"
                }
            })
            .Build();

        mockDataAccessProvider
            .Setup(dap => dap.GetUserByEmailAsync(EmailAddress))
            .ReturnsAsync(user);
        mockFileRetrievalService
            .Setup(cfg => cfg.GetPaginatedFileDataForUserAsync(user.EmailAddress, new List<string> { "114" }, 1, 20))
            .ReturnsAsync(fileData);

        // Act
        var result = await underTest.Index(new List<string> { "114" }, "", false);

        // Assert
        mockDataAccessProvider.Verify(dap => dap.MarkUserAsHavingLoggedInAsync(13));
    }

    [TestCase("Development", true)]
    [TestCase("DEV", true)]
    [TestCase("Staging", true)]
    [TestCase("Production", false)]
    public async Task Index_WhenUserIsServiceManager_OnlyShowsManualJobRunnersWhenNotOnProduction(string environmentName, bool showManualJobRunner)
    {
        // Arrange
        var user = new UserBuilder(EmailAddress)
            .WithHasLoggedIn(false)
            .WithRole(UserRole.ServiceManager)
            .Build();

        mockDataAccessProvider
            .Setup(dap => dap.GetUserByEmailAsync(EmailAddress))
            .ReturnsAsync(user);
        
        mockWebHostEnvironment.Setup(env => env.EnvironmentName).Returns(environmentName);
        mockDataAccessProvider.Setup(dap => dap.GetAllDeliveryPartnersAsync()).ReturnsAsync([]);

        var viewModel = new ServiceManagerHomepageViewModel([])
        {
            ShowManualJobRunner = showManualJobRunner
        };

        // Act
        var result = await underTest.Index([], "", false);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().BeEquivalentTo(viewModel);
    }
}