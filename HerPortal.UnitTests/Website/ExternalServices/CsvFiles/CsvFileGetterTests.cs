using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using FluentAssertions;
using HerPortal.BusinessLogic.ExternalServices.CsvFiles;
using HerPortal.BusinessLogic.ExternalServices.S3FileReader;
using HerPortal.BusinessLogic.Models;
using HerPortal.Data;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Tests.Website.ExternalServices.CsvFiles;

public class CsvFileGetterTests
{
    private Mock<ILogger<S3ReferralFileKeyService>> mockS3Logger;
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private Mock<IS3FileReader> mockFileReader;
    private CsvFileGetter underTest;
    
    private const int UserId = 13;

    [SetUp]
    public void Setup()
    {
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockS3Logger = new Mock<ILogger<S3ReferralFileKeyService>>();
        mockFileReader = new Mock<IS3FileReader>();
        var s3ReferralFileKeyService = new S3ReferralFileKeyService(mockS3Logger.Object);

        underTest = new CsvFileGetter(mockDataAccessProvider.Object, s3ReferralFileKeyService, mockFileReader.Object);
    }

    [Test]
    public async Task GetByCustodianCodesAsync_WhenCalledWithNeverDownloadedFiles_ReturnsFileData()
    {
        // Arrange
        mockDataAccessProvider
            .Setup(dap => dap.GetLastCsvFileDownloadsAsync(UserId))
            .ReturnsAsync(new List<CsvFileDownload>());
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_01.csv", LastModified = new DateTime(2023, 01, 31) },
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };
        var s3Objects910 = new List<S3Object>
        {
            new() { Key = "910/2023_01.csv", LastModified = new DateTime(2023, 01, 30) },
            new() { Key = "910/2023_02.csv", LastModified = new DateTime(2023, 02, 05) },
        };
        
        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);
        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("910")).ReturnsAsync(s3Objects910);

        // Act
        var result = (await underTest.GetByCustodianCodesAsync(new[] { "114", "910" }, UserId)).ToList();
        
        // Assert
        var expectedResult = new List<CsvFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 01, 31), null),
            new("114", 2, 2023, new DateTime(2023, 02, 04), null),
            new("910", 1, 2023, new DateTime(2023, 01, 30), null),
            new("910", 2, 2023, new DateTime(2023, 02, 05), null),
        };
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public async Task GetByCustodianCodesAsync_WhenCalledWithDownloadedFile_ReturnsFileData()
    {
        // Arrange
        mockDataAccessProvider
            .Setup(dap => dap.GetLastCsvFileDownloadsAsync(UserId))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new CsvFileDownload()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = UserId
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);

        var expectedResult = new List<CsvFileData>()
        {
            new CsvFileData("114", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };

        // Act
        var result = (await underTest.GetByCustodianCodesAsync(new[] { "114" }, UserId)).ToList();
        
        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }
}