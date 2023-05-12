using System;
using FluentAssertions;
using HerPortal.ExternalServices.CsvFiles;
using NUnit.Framework;

namespace Tests.Website.ExternalServices.CsvFiles;

[TestFixture]
public class CsvFileDataTests
{
    [TestCase("5210", 2023, 1, "5210/2023_01.csv")]
    [TestCase("505", 2024, 5, "505/2024_05.csv")]
    [TestCase("4215", 2020, 12, "4215/2020_12.csv")]
    public void CsvFileData_WhenGivenCustodianCodeYearAndMonth_ConstructsTheS3Key
    (
        string custodianCode,
        int year,
        int month,
        string expectedS3Key
    ) {
        // Arrange/Act
        var underTest = new CsvFileData
        (
            custodianCode,
            month,
            year,
            new DateTime(2024, 1, 1),
            null,
            true
        );
        
        // Assert
        underTest.S3Key.Should().Be(expectedS3Key);
    }
    
    [Test]
    public void CsvFileData_WhenLastDownloadedIsNull_SetsHasUpdatedSinceLastDownloadedToTrue()
    {
        // Arrange/Act
        var underTest = new CsvFileData
        (
            "505",
            1,
            2024,
            new DateTime(2024, 1, 1),
            null,
            true
        );
        
        // Assert
        underTest.HasUpdatedSinceLastDownload.Should().BeTrue();
    }
    
    [Test]
    public void CsvFileData_WhenLastDownloadedIsEarlierThanLastUpdated_SetsHasUpdatedSinceLastDownloadedToTrue()
    {
        // Arrange/Act
        var underTest = new CsvFileData
        (
            "505",
            1,
            2024,
            new DateTime(2023, 5, 2),
            new DateTime(2023, 5, 1),
            true
        );
        
        // Assert
        underTest.HasUpdatedSinceLastDownload.Should().BeTrue();
    }
    
    [Test]
    public void CsvFileData_WhenLastDownloadedIsLaterThanLastUpdated_SetsHasUpdatedSinceLastDownloadedToFalse()
    {
        // Arrange/Act
        var underTest = new CsvFileData
        (
            "505",
            1,
            2024,
            new DateTime(2023, 5, 1),
            new DateTime(2023, 5, 2),
            true
        );
        
        // Assert
        underTest.HasUpdatedSinceLastDownload.Should().BeFalse();
    }
}