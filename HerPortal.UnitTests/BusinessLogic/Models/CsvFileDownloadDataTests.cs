using System.Collections.Generic;
using FluentAssertions;
using HerPortal.BusinessLogic.Models;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.BusinessLogic.Models;

[TestFixture]
public class CsvFileDownloadDataTests
{
    [Test]
    public void CsvFileDownloadData_WhenThereAreNoDownloads_GivesNullLastDownload()
    {
        // Arrange
        var underTest = new CsvFileDownloadData
        {
            Downloads = new List<CsvFileDownload>(),
        };
        
        // Act
        var result = underTest.LastDownload;
        
        // Assert
        result.Should().BeNull();
    }
    
    [Test]
    public void CsvFileDownloadData_WhenThereAreNoDownloads_GivesNullLastDownloaded()
    {
        // Arrange
        var underTest = new CsvFileDownloadData
        {
            Downloads = new List<CsvFileDownload>(),
        };
        
        // Act
        var result = underTest.LastDownloaded;
        
        // Assert
        result.Should().BeNull();
    }
    
    [Test]
    public void CsvFileDownloadData_WhenThereAreNoDownloads_GivesNullLastDownloadedBy()
    {
        // Arrange
        var underTest = new CsvFileDownloadData
        {
            Downloads = new List<CsvFileDownload>(),
        };
        
        // Act
        var result = underTest.LastDownloadedBy;
        
        // Assert
        result.Should().BeNull();
    }
    
    [Test]
    public void CsvFileDownloadData_WhenThereAreDownloads_GivesLatestAsLastDownload()
    {
        // Arrange
        var underTest = new CsvFileDownloadData
        {
            Downloads = new List<CsvFileDownload>
            {
                new CsvFileDownloadBuilder(1).Build(),
                new CsvFileDownloadBuilder(2).Build(),
            },
        };
        
        // Act
        var result = underTest.LastDownload;
        
        // Assert
        result.Should().NotBeNull();
        result.DateTime.Month.Should().Be(2);
    }
    
    [Test]
    public void CsvFileDownloadData_WhenThereAreDownloads_GivesLatestAsLastDownloaded()
    {
        // Arrange
        var underTest = new CsvFileDownloadData
        {
            Downloads = new List<CsvFileDownload>
            {
                new CsvFileDownloadBuilder(1).Build(),
                new CsvFileDownloadBuilder(2).Build(),
            },
        };
        
        // Act
        var result = underTest.LastDownloaded;
        
        // Assert
        result.Should().NotBeNull();
        result!.Value.Month.Should().Be(2);
    }
    
    [Test]
    public void CsvFileDownloadData_WhenThereAreDownloads_GivesLatestAsLastDownloadedBy()
    {
        // Arrange
        var underTest = new CsvFileDownloadData
        {
            Downloads = new List<CsvFileDownload>
            {
                new CsvFileDownloadBuilder(1).WithUserWithId(1).Build(),
                new CsvFileDownloadBuilder(2).WithUserWithId(2).Build(),
            },
        };
        
        // Act
        var result = underTest.LastDownloadedBy;
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
    }
}
