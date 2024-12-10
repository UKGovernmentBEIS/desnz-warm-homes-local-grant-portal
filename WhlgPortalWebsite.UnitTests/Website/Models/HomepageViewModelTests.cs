﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPortal.Models;
using NUnit.Framework;
using Tests.Helpers;

namespace Tests.Website.Models;

[TestFixture]
public class HomepageViewModelTests
{
    private const string ValidCustodianCode = "505";
    private const string InvalidCustodianCode = "a";
    private const string InvalidConsortiumCode = "a";

    private string GetDummyPageLink(int pageNumber)
    {
        return $"link-{pageNumber}";
    }

    private string GetDummyDownloadLink(CsvFileData csvFileData)
    {
        return $"link-{csvFileData.Name}";
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
            Consortia = new List<Consortium>()
        };
        
        // Act
        var viewModel = new HomepageViewModel(user, new PaginatedFileData(), GetDummyPageLink, GetDummyDownloadLink);
        
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
            Consortia = ValidConsortiumGenerator.GetConsortiaWithDifferentCodes(0).ToList()
        };
        
        // Act
        var viewModel = new HomepageViewModel(user, new PaginatedFileData(), GetDummyPageLink, GetDummyDownloadLink);
        
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
    ) {
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
        var viewModel = new HomepageViewModel(user, new PaginatedFileData(), GetDummyPageLink, GetDummyDownloadLink);
        
        // Assert
        viewModel.ShouldShowFilters.Should().Be(expected);
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
        var csvFileData = new LocalAuthorityCsvFileData
        (
            ValidCustodianCode,
            month,
            year,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var viewModelCsvFile = new HomepageViewModel.CsvFile(csvFileData, "");
        
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
        var csvFileData = new LocalAuthorityCsvFileData
        (
            ValidCustodianCode,
            1,
            2024,
            new DateTime(year, month, day),
            null
        );
        
        // Act
        var viewModelCsvFile = new HomepageViewModel.CsvFile(csvFileData, "");
        
        // Assert
        viewModelCsvFile.LastUpdatedText.Should().Be(expectedLastUpdatedString);
    }
    
    [TestCase("5210", "Camden Council")]
    [TestCase("505", "Cambridge City Council")]
    [TestCase("4215", "Manchester City Council")]
    public void HomepageViewModelCsvFile_WhenValidCustodianCodeIsGiven_GetsTheLocalAuthorityName
    (
        string custodianCode,
        string expectedLocalAuthorityName
    ) {
        // Arrange
        var csvFileData = new LocalAuthorityCsvFileData
        (
            custodianCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var viewModelCsvFile = new HomepageViewModel.CsvFile(csvFileData, "");
        
        // Assert
        viewModelCsvFile.Name.Should().Be(expectedLocalAuthorityName);
    }
    
    [Test]
    public void HomepageViewModelCsvFile_WhenInvalidCustodianCodeIsGiven_ThrowsException()
    {
        // Arrange
        var csvFileData = new LocalAuthorityCsvFileData
        (
            InvalidCustodianCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var act = () => new HomepageViewModel.CsvFile(csvFileData, "");
        
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [TestCase("C_0022", "Liverpool City Region Combined Authority (Consortium)")]
    [TestCase("C_0006", "Cambridge City Council (Consortium)")]
    [TestCase("C_0004", "Broadland District Council (Consortium)")]
    public void HomepageViewModelCsvFile_WhenValidConsortiumCodeIsGiven_GetsTheConsortiumName
    (
        string consortiumCode,
        string expectedLocalAuthorityName
    ) {
        // Arrange
        var csvFileData = new ConsortiumCsvFileData
        (
            consortiumCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var viewModelCsvFile = new HomepageViewModel.CsvFile(csvFileData, "");
        
        // Assert
        viewModelCsvFile.Name.Should().Be(expectedLocalAuthorityName);
    }
    
    [Test]
    public void HomepageViewModelCsvFile_WhenInvalidConsortiumCodeIsGiven_ThrowsException()
    {
        // Arrange
        var csvFileData = new ConsortiumCsvFileData
        (
            InvalidConsortiumCode,
            1,
            2024,
            new DateTime(2023, 1, 1),
            null
        );
        
        // Act
        var act = () => new HomepageViewModel.CsvFile(csvFileData, "");
        
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
