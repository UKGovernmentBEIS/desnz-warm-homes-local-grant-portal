using System;
using System.Security;
using System.Threading.Tasks;
using FluentAssertions;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;
using WhlgPortalWebsite.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.Website.Controllers;

[TestFixture]
public class CsvFileControllerTests
{
    private Mock<ILogger<CsvFileController>> mockCsvLogger;
    private CsvFileController underTest;
    private Mock<ICsvFileService> mockCsvFileService;
    
    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockCsvLogger = new Mock<ILogger<CsvFileController>>();
        mockCsvFileService = new Mock<ICsvFileService>();

        underTest = new CsvFileController(mockCsvFileService.Object, mockCsvLogger.Object);
        underTest.ControllerContext.HttpContext = new HttpContextBuilder(EmailAddress).Build();
    }

    [Test]
    public async Task GetLaCsvFile_WhenCalledForUnauthorisedCustodianCode_ReturnsUnauthorised()
    {
        // Arrange
        mockCsvFileService
            .Setup(cfs => cfs.GetLocalAuthorityFileForDownloadAsync("115", 2023, 11, EmailAddress))
            .ThrowsAsync(new SecurityException());
        
        // Act
        var result = await underTest.GetLaCsvFile("115", 2023, 11);
        
        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Test]
    public async Task GetLaCsvFile_WhenCalledForMissingFile_ReturnsNotFound()
    {
        // Arrange
        mockCsvFileService
            .Setup(cfs => cfs.GetLocalAuthorityFileForDownloadAsync("115", 2023, 11, EmailAddress))
            .ThrowsAsync(new ArgumentOutOfRangeException());
        
        // Act
        var result = await underTest.GetLaCsvFile("115", 2023, 11);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetConsortiumCsvFile_WhenCalledForUnauthorisedCustodianCode_ReturnsUnauthorised()
    {
        // Arrange
        mockCsvFileService
            .Setup(cfs => cfs.GetConsortiumFileForDownloadAsync("C_0001", 2023, 11, EmailAddress))
            .ThrowsAsync(new SecurityException());
        
        // Act
        var result = await underTest.GetConsortiumCsvFile("C_0001", 2023, 11);
        
        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Test]
    public async Task GetConsortiumCsvFile_WhenCalledForMissingFile_ReturnsNotFound()
    {
        // Arrange
        mockCsvFileService
            .Setup(cfs => cfs.GetConsortiumFileForDownloadAsync("C_0001", 2023, 11, EmailAddress))
            .ThrowsAsync(new ArgumentOutOfRangeException());
        
        // Act
        var result = await underTest.GetConsortiumCsvFile("C_0001", 2023, 11);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
