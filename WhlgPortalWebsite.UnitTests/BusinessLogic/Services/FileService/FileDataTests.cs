using System;
using FluentAssertions;
using NUnit.Framework;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;

namespace Tests.BusinessLogic.Services.FileService;

[TestFixture]
public class FileDataTests
{
    [Test]
    public void FileData_WhenLastDownloadedIsNull_SetsHasUpdatedSinceLastDownloadedToTrue()
    {
        // Arrange/Act
        var underTest = new LocalAuthorityFileData
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
    public void FileData_WhenLastDownloadedIsEarlierThanLastUpdated_SetsHasUpdatedSinceLastDownloadedToTrue()
    {
        // Arrange/Act
        var underTest = new LocalAuthorityFileData
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
    public void FileData_WhenLastDownloadedIsLaterThanLastUpdated_SetsHasUpdatedSinceLastDownloadedToFalse()
    {
        // Arrange/Act
        var underTest = new LocalAuthorityFileData
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
    public void FileData_WhenDatedBeforeHUG2Shutdown_ContainsLegacyReferralsIsTrue(int month, int year,
        bool expectedContainsLegacyReferrals)
    {
        // Arrange/Act
        var underTest = new LocalAuthorityFileData
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