using System;
using System.Security;
using System.Threading.Tasks;
using FluentAssertions;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPortal.Controllers;
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
    public async Task GetCsvFile_WhenCalledForUnauthorisedCustodianCode_ReturnsUnauthorised()
    {
        // Arrange
        mockCsvFileService
            .Setup(cfs => cfs.GetFileForDownloadAsync("115", 2023, 11, EmailAddress))
            .ThrowsAsync(new SecurityException());
        
        // Act
        var result = await underTest.GetCsvFile("115", 2023, 11);
        
        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Test]
    public async Task GetCsvFile_WhenCalledForMissingFile_ReturnsNotFound()
    {
        // Arrange
        mockCsvFileService
            .Setup(cfs => cfs.GetFileForDownloadAsync("115", 2023, 11, EmailAddress))
            .ThrowsAsync(new ArgumentOutOfRangeException());
        
        // Act
        var result = await underTest.GetCsvFile("115", 2023, 11);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
