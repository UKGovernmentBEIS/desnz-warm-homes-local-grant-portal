using System;
using FluentAssertions;
using NUnit.Framework;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;

namespace Tests.BusinessLogic.Services.CsvFileService;

[TestFixture]
public class CsvFileDataTests
{
    [Test]
    public void CsvFileData_WhenLastDownloadedIsNull_SetsHasUpdatedSinceLastDownloadedToTrue()
    {
        // Arrange/Act
        var underTest = new LocalAuthorityCsvFileData
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
        var underTest = new LocalAuthorityCsvFileData
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
        var underTest = new LocalAuthorityCsvFileData
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

    [TestCase(1, 2024, true)]
    [TestCase(3, 2024, true)]
    [TestCase(4, 2024, true)]
    [TestCase(5, 2024, true)]
    [TestCase(1, 2025, true)]
    [TestCase(2, 2025, true)]
    [TestCase(3, 2025, false)]
    [TestCase(4, 2025, false)]
    [TestCase(1, 2026, false)]
    [TestCase(2, 2026, false)]
    [TestCase(3, 2026, false)]
    [TestCase(4, 2026, false)]
    public void CsvFileData_WhenDatedBeforeHUG2Shutdown_ContainsLegacyReferralsIsTrue(int month, int year,
        bool expectedContainsLegacyReferrals)
    {
        // Arrange/Act
        var underTest = new LocalAuthorityCsvFileData
        (
            "505",
            month,
            year,
            new DateTime(2024, 1, 1),
            null
        );

        // Assert
        underTest.ContainsLegacyReferrals.Should().Be(expectedContainsLegacyReferrals);
    }
}