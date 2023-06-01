using System;
using FluentAssertions;
using HerPortal.ExternalServices.CsvFiles;
using NUnit.Framework;

namespace Tests.Website.ExternalServices.CsvFiles;

[TestFixture]
public class CsvFileDataTests
{
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
            null
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
            new DateTime(2023, 5, 1)
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
            new DateTime(2023, 5, 2)
        );
        
        // Assert
        underTest.HasUpdatedSinceLastDownload.Should().BeFalse();
    }
}
