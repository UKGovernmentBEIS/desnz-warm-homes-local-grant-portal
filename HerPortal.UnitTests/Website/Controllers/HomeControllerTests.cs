using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPortal.Controllers;
using HerPortal.Data;
using HerPortal.DataStores;
using HerPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.Website.Controllers;

[TestFixture]
public class HomeFileControllerTests
{
    private Mock<ILogger<UserDataStore>> mockUserDataLogger;
    private Mock<ILogger<HomeController>> mockHomeLogger;
    private HomeController underTest;
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private Mock<ICsvFileService> mockCsvFileService;

    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockUserDataLogger = new Mock<ILogger<UserDataStore>>();
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockHomeLogger = new Mock<ILogger<HomeController>>();
        mockCsvFileService = new Mock<ICsvFileService>();
        var userDataStore = new UserDataStore(mockDataAccessProvider.Object, mockUserDataLogger.Object);

        underTest = new HomeController(userDataStore, mockCsvFileService.Object, mockHomeLogger.Object);
        underTest.ControllerContext.HttpContext = new HttpContextBuilder(EmailAddress).Build();
    }

    [Test]
    public async Task Index_WhenCalledWithoutFilter_ShowsAllFiles()
    {
        // Arrange
        var files = new List<CsvFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), null),
            new("114", 2, 2023, new DateTime(2023, 2, 3), null),
            new("910", 1, 2023, new DateTime(2023, 1, 31), null),
        };

        var user = new UserBuilder(EmailAddress)
            .WithLocalAuthorities(new List<LocalAuthority>
            {
                new()
                {
                    Id = 1,
                    CustodianCode = "114"
                },
                new()
                {
                    Id = 2,
                    CustodianCode = "910"
                }
            })
            .Build();

        mockDataAccessProvider
            .Setup(dap => dap.GetUserByEmailAsync(EmailAddress))
            .ReturnsAsync(user);
        mockCsvFileService
            .Setup(cfg => cfg.GetByCustodianCodesAsync(new string[] { "114", "910" }, user.Id))
            .ReturnsAsync(files);
        
        // Act
        var result = await underTest.Index(new List<string>());
        
        // Assert
        result.As<ViewResult>().Model.As<HomepageViewModel>().CsvFiles.Count().Should().Be(3);
    }
    
    
    [Test]
    public async Task Index_WhenCalledWithFilter_RemovesFilteredFiles()
    {
        // Arrange
        var files = new List<CsvFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), null),
            new("114", 2, 2023, new DateTime(2023, 2, 3), null),
            new("910", 1, 2023, new DateTime(2023, 1, 31), null),
        };
        var user = new UserBuilder(EmailAddress)
            .WithLocalAuthorities(new List<LocalAuthority>
            {
                new()
                {
                    Id = 1,
                    CustodianCode = "114"
                },
                new()
                {
                    Id = 2,
                    CustodianCode = "910"
                }
            })
            .Build();

        mockDataAccessProvider
            .Setup(dap => dap.GetUserByEmailAsync(EmailAddress))
            .ReturnsAsync(user);
        mockCsvFileService
            .Setup(cfg => cfg.GetByCustodianCodesAsync(new [] { "114", "910" }, user.Id))
            .ReturnsAsync(files);
        
        // Act
        var result = await underTest.Index(new List<string> { "114" });
        
        // Assert
        result.As<ViewResult>().Model.As<HomepageViewModel>().CsvFiles.Count().Should().Be(2);
    }
    
    [Test]
    public async Task Index_WhenUserViewsForTheFirstTime_SetsLoggedInFlag()
    {
        // Arrange
        var files = new List<CsvFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), null)
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
            .Setup(cfg => cfg.GetByCustodianCodesAsync(new string[] { "114" }, 13))
            .ReturnsAsync(files);
        
        // Act
        var result = await underTest.Index(new List<string> { "114" });
        
        // Assert
        mockDataAccessProvider.Verify(dap => dap.MarkUserAsHavingLoggedInAsync(13));
    }
}
