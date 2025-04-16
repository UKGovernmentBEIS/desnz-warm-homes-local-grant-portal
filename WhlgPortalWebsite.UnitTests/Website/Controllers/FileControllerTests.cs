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
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Enums;

namespace Tests.Website.Controllers;

[TestFixture]
public class FileControllerTests
{
    private Mock<ILogger<FileController>> mockFileControllerLogger;
    private FileController underTest;
    private Mock<IFileRetrievalService> mockFileService;
    private Mock<IStreamService> mockFileStreamService;
    
    private const string EmailAddress = "test@example.com";

    [SetUp]
    public void Setup()
    {
        mockFileControllerLogger = new Mock<ILogger<FileController>>();
        mockFileService = new Mock<IFileRetrievalService>();
        mockFileStreamService = new Mock<IStreamService>();

        underTest = new FileController(mockFileService.Object, mockFileControllerLogger.Object, mockFileStreamService.Object);
        underTest.ControllerContext.HttpContext = new HttpContextBuilder(EmailAddress).Build();
    }

    [TestCase(FileType.Csv)]
    [TestCase(FileType.Xlsx)]
    public async Task GetLaFile_WhenCalledForUnauthorisedCustodianCode_ReturnsUnauthorised(FileType fileType)
    {
        // Arrange
        mockFileService
            .Setup(cfs => cfs.GetLocalAuthorityFileForDownloadAsync("115", 2023, 11, EmailAddress))
            .ThrowsAsync(new SecurityException());
        
        // Act
        var result = await underTest.GetLaFile("115", 2023, 11, fileType.ToString());
        
        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [TestCase(FileType.Csv)]
    [TestCase(FileType.Xlsx)]
    public async Task GetLaFile_WhenCalledForMissingFile_ReturnsNotFound(FileType fileType)
    {
        // Arrange
        mockFileService
            .Setup(cfs => cfs.GetLocalAuthorityFileForDownloadAsync("115", 2023, 11, EmailAddress))
            .ThrowsAsync(new ArgumentOutOfRangeException());
        
        // Act
        var result = await underTest.GetLaFile("115", 2023, 11, fileType.ToString());
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestCase(FileType.Csv)]
    [TestCase(FileType.Xlsx)]
    public async Task GetConsortiumFile_WhenCalledForUnauthorisedCustodianCode_ReturnsUnauthorised(FileType fileType)
    {
        // Arrange
        mockFileService
            .Setup(cfs => cfs.GetConsortiumFileForDownloadAsync("C_0001", 2023, 11, EmailAddress))
            .ThrowsAsync(new SecurityException());
        
        // Act
        var result = await underTest.GetConsortiumFile("C_0001", 2023, 11, fileType.ToString());
        
        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [TestCase(FileType.Csv)]
    [TestCase(FileType.Xlsx)]
    public async Task GetConsortiumFile_WhenCalledForMissingFile_ReturnsNotFound(FileType fileType)
    {
        // Arrange
        mockFileService
            .Setup(cfs => cfs.GetConsortiumFileForDownloadAsync("C_0001", 2023, 11, EmailAddress))
            .ThrowsAsync(new ArgumentOutOfRangeException());
        
        // Act
        var result = await underTest.GetConsortiumFile("C_0001", 2023, 11, fileType.ToString());
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
