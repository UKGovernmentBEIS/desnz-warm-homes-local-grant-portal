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
public class FileControllerTests
{
    private Mock<ILogger<FileController>> mockCsvLogger;
    private FileController underTest;
    private Mock<IFileService> mockCsvFileService;
    
    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockCsvLogger = new Mock<ILogger<FileController>>();
        mockCsvFileService = new Mock<IFileService>();

        underTest = new FileController(mockCsvFileService.Object, mockCsvLogger.Object);
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
        var result = await underTest.GetLaFile("115", 2023, 11);
        
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
        var result = await underTest.GetLaFile("115", 2023, 11);
        
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
        var result = await underTest.GetConsortiumFile("C_0001", 2023, 11);
        
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
        var result = await underTest.GetConsortiumFile("C_0001", 2023, 11);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
