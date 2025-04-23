using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tests.Helpers;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Models;

[TestFixture]
public class AuthorityStaffHomepageViewModelTests
{
    private const string ValidCustodianCode = "505";
    private const string InvalidCustodianCode = "a";
    private const string InvalidConsortiumCode = "a";

    private string GetDummyPageLink(int pageNumber)
    {
        return $"link-{pageNumber}";
    }

    private string GetDummyDownloadLink(FileData fileData, FileType fileType)
    {
        return $"link-{fileData.Name}.{fileType.ToString()}";
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void HomepageViewModel_OnlyWhenCreatedForUserThatHasntLoggedInBefore_ShouldShowBanner
    (
        bool hasUserLoggedIn,
        bool shouldShowBanner
    )
    {
        // Arrange
        var user = new User
        {
            HasLoggedIn = hasUserLoggedIn,
            LocalAuthorities = new List<LocalAuthority>(),
            Consortia = new List<Consortium>()
        };

        // Act
        var viewModel = new AuthorityStaffHomepageViewModel(user, new PaginatedFileData(), GetDummyPageLink, GetDummyDownloadLink);

        // Assert
        viewModel.ShouldShowBanner.Should().Be(shouldShowBanner);
    }

    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    [TestCase(4, true)]
    [TestCase(100, true)]
    public void HomepageViewModel_OnlyWhenUserHasOneLocalAuthority_ShouldNotShowFilters
    (
        int numberOfLas,
        bool expected
    )
    {
        // Arrange
        var user = new User
        {
            HasLoggedIn = true,
            LocalAuthorities = ValidLocalAuthorityGenerator
                .GetLocalAuthoritiesWithDifferentCodes(numberOfLas)
                .ToList(),
            Consortia = ValidConsortiumGenerator.GetConsortiaWithDifferentCodes(0).ToList()
        };

        // Act
        var viewModel = new AuthorityStaffHomepageViewModel(user, new PaginatedFileData(), GetDummyPageLink, GetDummyDownloadLink);

        // Assert
        viewModel.ShouldShowFilters.Should().Be(expected);
    }

    [TestCase(0, 1, true)]
    [TestCase(1, 1, true)]
    public void HomepageViewModel_WhenUserHasConsortium_ShouldShowFilters
    (
        int numberOfLas,
        int numberOfConsortia,
        bool expected
    )
    {
        // Arrange
        var user = new User
        {
            HasLoggedIn = true,
            LocalAuthorities = ValidLocalAuthorityGenerator
                .GetLocalAuthoritiesWithDifferentCodes(numberOfLas)
                .ToList(),
            Consortia = ValidConsortiumGenerator.GetConsortiaWithDifferentCodes(numberOfConsortia).ToList()
        };

        // Act
        var viewModel = new AuthorityStaffHomepageViewModel(user, new PaginatedFileData(), GetDummyPageLink, GetDummyDownloadLink);

        // Assert
        viewModel.ShouldShowFilters.Should().Be(expected);
    }

    [TestCase(1, 2023, "January 2023")]
    [TestCase(4, 2020, "April 2020")]
    [TestCase(12, 2025, "December 2025")]
    public void HomepageViewModelFile_WhenMonthAndYearAreGiven_ConvertsThemToAStringWithFullMonthAndYear
    (
        int month,
        int year,
        string expectedDateString
    )
    {
        // Arrange
        var fileData = new LocalAuthorityFileData
        (
            ValidCustodianCode,
            month,
            year,
            new DateTime(2023, 1, 1),
            null
        );

        // Act
        var viewModelFiles = new AuthorityStaffHomepageViewModel.ReferralDownloadListing(fileData, "", "");

        // Assert
        viewModelFiles.MonthAndYearText.Should().Be(expectedDateString);
    }

    [TestCase(26, 1, 2023, "26/01/23")]
    [TestCase(22, 5, 2020, "22/05/20")]
    [TestCase(19, 12, 2025, "19/12/25")]
    public void HomepageViewModelFile_WhenLastUpdatedIsGiven_ConvertsItToAStringWith2DigitsAndSlashes
    (
        int day,
        int month,
        int year,
        string expectedLastUpdatedString
    )
    {
        // Arrange
        var fileData = new LocalAuthorityFileData
        (
            ValidCustodianCode,
            1,
            2024,
            new DateTime(year, month, day),
            null
        );

        // Act
        var viewModelFiles = new AuthorityStaffHomepageViewModel.ReferralDownloadListing(fileData, "", "");

        // Assert
        viewModelFiles.LastUpdatedText.Should().Be(expectedLastUpdatedString);
    }

    [TestCase("5210", "Camden Council")]
    [TestCase("505", "Cambridge City Council")]
    [TestCase("4215", "Manchester City Council")]
    public void HomepageViewModelFile_WhenValidCustodianCodeIsGiven_GetsTheLocalAuthorityName
    (
        string custodianCode,
        string expectedLocalAuthorityName
    )
    {
        // Arrange
        var fileData = new LocalAuthorityFileData
        (
            custodianCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );

        // Act
        var viewModelFiles = new AuthorityStaffHomepageViewModel.ReferralDownloadListing(fileData, "", "");

        // Assert
        viewModelFiles.Name.Should().Be(expectedLocalAuthorityName);
    }

    [Test]
    public void HomepageViewModelFile_WhenInvalidCustodianCodeIsGiven_ThrowsException()
    {
        // Arrange
        var fileData = new LocalAuthorityFileData
        (
            InvalidCustodianCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );

        // Act
        var act = () => new AuthorityStaffHomepageViewModel.ReferralDownloadListing(fileData, "", "");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase("C_0016", "Liverpool City Region Combined Authority (Consortium)")]
    [TestCase("C_0004", "Cambridge City Council (Consortium)")]
    [TestCase("C_0003", "Broadland District Council (Consortium)")]
    public void HomepageViewModelFile_WhenValidConsortiumCodeIsGiven_GetsTheConsortiumName
    (
        string consortiumCode,
        string expectedLocalAuthorityName
    )
    {
        // Arrange
        var fileData = new ConsortiumFileData
        (
            consortiumCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );

        // Act
        var viewModelFiles = new AuthorityStaffHomepageViewModel.ReferralDownloadListing(fileData, "", "");

        // Assert
        viewModelFiles.Name.Should().Be(expectedLocalAuthorityName);
    }

    [Test]
    public void HomepageViewModelFile_WhenInvalidConsortiumCodeIsGiven_ThrowsException()
    {
        // Arrange
        var fileData = new ConsortiumFileData
        (
            InvalidConsortiumCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );

        // Act
        var act = () => new AuthorityStaffHomepageViewModel.ReferralDownloadListing(fileData, "", "");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}