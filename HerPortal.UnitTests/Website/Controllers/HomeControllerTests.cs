using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HerPortal.BusinessLogic;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPortal.Controllers;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.Website.Controllers;

[TestFixture]
public class HomeFileControllerTests
{
    private HomeController underTest;
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private Mock<ICsvFileService> mockCsvFileService;

    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockCsvFileService = new Mock<ICsvFileService>();
        var userDataStore = new UserService(mockDataAccessProvider.Object);

        underTest = new HomeController(userDataStore, mockCsvFileService.Object);
        underTest.ControllerContext.HttpContext = new HttpContextBuilder(EmailAddress).Build();
    }

    [Test]
    public async Task Index_WhenUserViewsForTheFirstTime_SetsLoggedInFlag()
    {
        // Arrange
        var fileData = new PaginatedFileData
        {
            CurrentPage = 1,
            MaximumPage = 1,
            FileData = new List<CsvFileData>()
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
        mockCsvFileService
            .Setup(cfg => cfg.GetPaginatedFileDataForUserAsync(user.EmailAddress, new List<string> { "114"}, 1, 20))
            .ReturnsAsync(fileData);
        
        // Act
        var result = await underTest.Index(new List<string> { "114" });
        
        // Assert
        mockDataAccessProvider.Verify(dap => dap.MarkUserAsHavingLoggedInAsync(13));
    }
}
