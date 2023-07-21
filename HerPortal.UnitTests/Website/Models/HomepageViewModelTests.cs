using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPortal.Models;
using NUnit.Framework;
using Tests.Builders;
using Tests.Helpers;

namespace Tests.Website.Models;

[TestFixture]
public class HomepageViewModelTests
{
    private const string ValidCustodianCode = "505";
    private const string InvalidCustodianCode = "a";

    private string GetDummyLink(int pageNumber)
    {
        return $"link-{pageNumber}";
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void HomepageViewModel_OnlyWhenCreatedForUserThatHasntLoggedInBefore_ShouldShowBanner
    (
        bool hasUserLoggedIn,
        bool shouldShowBanner
    ) {
        // Arrange
        var user = new User
        {
            HasLoggedIn = hasUserLoggedIn,
            LocalAuthorities = new List<LocalAuthority>(),
        };
        
        // Act
        var viewModel = new HomepageViewModel(user, new PaginatedFileData(), GetDummyLink);
        
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
    ) {
        // Arrange
        var user = new User
        {
            HasLoggedIn = true,
            LocalAuthorities = ValidLocalAuthorityGenerator
                .GetLocalAuthoritiesWithDifferentCodes(numberOfLas)
                .ToList(),
        };
        
        // Act
        var viewModel = new HomepageViewModel(user, new PaginatedFileData(), GetDummyLink);
        
        // Assert
        viewModel.ShouldShowFilters.Should().Be(expected);
    }
    
    [Test]
    public void HomepageViewModel_WithOnePageOfResults_HidesPagination()
    {
        // Arrange
        var user = new UserBuilder("test@example.com").Build();
        var fileData = new PaginatedFileData()
        {
            CurrentPage = 1,
            MaximumPage = 1,
            FileData = new List<CsvFileData>()
        };
        
        // Act
        var viewModel = new HomepageViewModel(user, new PaginatedFileData(), GetDummyLink);
        
        // Assert
        viewModel.PaginationDetails.Should().BeNull();
    }
    
    [TestCase(1, 2, false, true, "1", "2")]
    [TestCase(2, 2, true, false, "1", "2")]
    [TestCase(1, 3, false, true, "1", "2", "3")]
    [TestCase(2, 3, true, true, "1", "2", "3")]
    [TestCase(3, 3, true, false, "1", "2", "3")]
    [TestCase(1, 4, false, true, "1", "2", "...", "4")]
    [TestCase(2, 4, true, true, "1", "2", "3", "4")]
    [TestCase(3, 4, true, true, "1", "2", "3", "4")]
    [TestCase(4, 4, true, false, "1", "...", "3", "4")]
    [TestCase(1, 5, false, true, "1", "2", "...", "5")]
    [TestCase(2, 5, true, true, "1", "2", "3", "...", "5")]
    [TestCase(3, 5, true, true, "1", "2", "3", "4", "5")]
    [TestCase(4, 5, true, true, "1", "...", "3", "4", "5")]
    [TestCase(5, 5, true, false, "1", "...", "4", "5")]
    [TestCase(1, 6, false, true, "1", "2", "...", "6")]
    [TestCase(2, 6, true, true, "1", "2", "3", "...", "6")]
    [TestCase(3, 6, true, true, "1", "2", "3", "4", "...", "6")]
    [TestCase(4, 6, true, true, "1", "...", "3", "4", "5", "6")]
    [TestCase(5, 6, true, true, "1", "...", "4", "5", "6")]
    [TestCase(6, 6, true, false, "1", "...", "5", "6")]
    [TestCase(4, 7, true, true, "1", "...", "3", "4", "5", "...", "7")]
    public void HomepageViewModel_WithMultiplePageOfResults_ShowsCorrectLinks(
        int currentPage,
        int maxPage,
        bool shouldShowPreviousLink,
        bool shouldShowNextLink,
        params string[] expectedLinkTexts)
    {
        // Arrange
        var user = new UserBuilder("test@example.com").Build();
        var fileData = new PaginatedFileData()
        {
            CurrentPage = currentPage,
            MaximumPage = maxPage,
            FileData = new List<CsvFileData>()
        };
        
        // Act
        var viewModel = new HomepageViewModel(user, fileData, GetDummyLink);
        
        // Assert
        if (shouldShowPreviousLink)
        {
            viewModel.PaginationDetails.Previous.Should().NotBeNull();
        }
        else
        {
            viewModel.PaginationDetails.Previous.Should().BeNull();
        }
        if (shouldShowNextLink)
        {
            viewModel.PaginationDetails.Next.Should().NotBeNull();
        }
        else
        {
            viewModel.PaginationDetails.Next.Should().BeNull();
        }

        var links = viewModel.PaginationDetails.Items;
        links.Count.Should().Be(expectedLinkTexts.Length);

        for (var i = 0; i < links.Count; i++)
        {
            if (expectedLinkTexts[i] == "...")
            {
                links[i].Ellipsis.Should().BeTrue();
            }
            else
            {
                links[i].Ellipsis.Should().BeFalse();
                links[i].Number.Should().Be(expectedLinkTexts[i]);    
            }
        }
    }

    [TestCase(1, 2023, "January 2023")]
    [TestCase(4, 2020, "April 2020")]
    [TestCase(12, 2025, "December 2025")]
    public void HomepageViewModelCsvFile_WhenMonthAndYearAreGiven_ConvertsThemToAStringWithFullMonthAndYear
    (
        int month,
        int year,
        string expectedDateString
    ) {
        // Arrange
        var csvFileData = new CsvFileData
        (
            ValidCustodianCode,
            month,
            year,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var viewModelCsvFile = new HomepageViewModel.CsvFile(csvFileData);
        
        // Assert
        viewModelCsvFile.MonthAndYearText.Should().Be(expectedDateString);
    }
    
    [TestCase(26, 1, 2023, "26/01/23")]
    [TestCase(22, 5, 2020, "22/05/20")]
    [TestCase(19, 12, 2025, "19/12/25")]
    public void HomepageViewModelCsvFile_WhenLastUpdatedIsGiven_ConvertsItToAStringWith2DigitsAndSlashes
    (
        int day,
        int month,
        int year,
        string expectedLastUpdatedString
    ) {
        // Arrange
        var csvFileData = new CsvFileData
        (
            ValidCustodianCode,
            1,
            2024,
            new DateTime(year, month, day),
            null
        );
        
        // Act
        var viewModelCsvFile = new HomepageViewModel.CsvFile(csvFileData);
        
        // Assert
        viewModelCsvFile.LastUpdatedText.Should().Be(expectedLastUpdatedString);
    }
    
    [TestCase("5210", "Camden")]
    [TestCase("505", "Cambridge")]
    [TestCase("4215", "Manchester")]
    public void HomepageViewModelCsvFile_WhenValidCustodianCodeIsGiven_GetsTheLocalAuthorityName
    (
        string custodianCode,
        string expectedLocalAuthorityName
    ) {
        // Arrange
        var csvFileData = new CsvFileData
        (
            custodianCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var viewModelCsvFile = new HomepageViewModel.CsvFile(csvFileData);
        
        // Assert
        viewModelCsvFile.LocalAuthorityName.Should().Be(expectedLocalAuthorityName);
    }
    
    [Test]
    public void HomepageViewModelCsvFile_WhenInvalidCustodianCodeIsGiven_ThrowsException()
    {
        // Arrange
        var csvFileData = new CsvFileData
        (
            InvalidCustodianCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var act = () => new HomepageViewModel.CsvFile(csvFileData);
        
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
