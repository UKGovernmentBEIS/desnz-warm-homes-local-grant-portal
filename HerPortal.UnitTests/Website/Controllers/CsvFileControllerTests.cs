using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using HerPortal.BusinessLogic.ExternalServices.CsvFiles;
using HerPortal.BusinessLogic.Models;
using HerPortal.Controllers;
using HerPortal.Data;
using HerPortal.DataStores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.Website.Controllers;

[TestFixture]
public class CsvFileControllerTests
{
    private Mock<ILogger<UserDataStore>> mockUserDataLogger;
    private Mock<ILogger<CsvFileController>> mockCsvLogger;
    private CsvFileController underTest;
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private Mock<ICsvFileGetter> mockCsvFileGetter;
    
    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockUserDataLogger = new Mock<ILogger<UserDataStore>>();
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockCsvLogger = new Mock<ILogger<CsvFileController>>();
        mockCsvFileGetter = new Mock<ICsvFileGetter>();
        var userDataStore = new UserDataStore(mockDataAccessProvider.Object, mockUserDataLogger.Object);

        underTest = new CsvFileController(userDataStore, mockCsvFileGetter.Object, mockCsvLogger.Object);
        underTest.ControllerContext.HttpContext = new HttpContextBuilder(EmailAddress).Build();
    }

    [Test]
    public async Task GetCsvFile_WhenCalledForUnauthorisedCustodianCode_ReturnsUnauthorised()
    {
        // Arrange
        var user = new UserBuilder(EmailAddress)
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
        
        // Act
        var result = await underTest.GetCsvFile("115", 2023, 11);
        
        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
