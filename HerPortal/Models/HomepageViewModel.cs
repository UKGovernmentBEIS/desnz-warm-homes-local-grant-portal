using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services.CsvFileService;

namespace HerPortal.Models;

public class HomepageViewModel
{
    public class CsvFile
    {
        public string CustodianCode { get; }
        public int Year { get; }
        public int Month { get; }
        public string MonthAndYearText => new DateOnly(Year, Month, 1).ToString("MMMM yyyy");
        public string LocalAuthorityName => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[CustodianCode];
        public string LastUpdatedText { get; }
        public bool HasNewUpdates { get; }

        public CsvFile(CsvFileData csvFileData)
        {
            if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(csvFileData.CustodianCode))
            {
                throw new ArgumentOutOfRangeException(nameof(csvFileData.CustodianCode), csvFileData.CustodianCode,
                    "The given custodian code is not known.");
            }

            CustodianCode = csvFileData.CustodianCode;
            Year = csvFileData.Year;
            Month = csvFileData.Month;
            LastUpdatedText = csvFileData.LastUpdated.ToString("dd/MM/yy");
            HasNewUpdates = csvFileData.HasUpdatedSinceLastDownload;
        }
    }
    
    public bool ShouldShowBanner { get; }
    public bool ShouldShowFilters { get; }
    public bool UserHasNewUpdates { get; }
    public List<string> CustodianCodes { get; }
    public Dictionary<string, LabelViewModel> LocalAuthorityCheckboxLabels { get; }
    public IEnumerable<CsvFile> CsvFiles { get; }
    public PaginationViewModel PaginationDetails { get; } 

    public HomepageViewModel(User user, PaginatedFileData paginatedFileData, Func<int, string> pageLinkGenerator)
    {
        ShouldShowBanner = !user.HasLoggedIn;
        ShouldShowFilters = user.LocalAuthorities.Count >= 2;
        CustodianCodes = user.LocalAuthorities.Select(la => la.CustodianCode).ToList();
        LocalAuthorityCheckboxLabels = new Dictionary<string, LabelViewModel>(user.LocalAuthorities
            .Select(la => new KeyValuePair<string, LabelViewModel>
                (
                    la.CustodianCode,
                    new LabelViewModel
                    {
                        Text = LocalAuthorityData.LocalAuthorityNamesByCustodianCode[la.CustodianCode],
                    }
                )
            )
            .OrderBy(kvp => kvp.Value.Text)
        );
        CsvFiles = paginatedFileData.FileData.Select(cf => new CsvFile(cf));

        UserHasNewUpdates = paginatedFileData.UserHasUndownloadedFiles;

        PaginationDetails = GetPaginationDetails(paginatedFileData, pageLinkGenerator);
    }

    private PaginationViewModel GetPaginationDetails(PaginatedFileData paginatedFileData, Func<int,string> pageLinkGenerator)
    {
        if (paginatedFileData.MaximumPage <= 1)
        {
            return null;
        }
        
        var paginationLinks = new List<PaginationItemViewModel>
        {
            new ()
            {
                Number = "1",
                Current = paginatedFileData.CurrentPage == 1,
                Href = pageLinkGenerator(1)
            }
        };

        for (var pageNumber = 2; pageNumber < paginatedFileData.MaximumPage; pageNumber++)
        {
            if (pageNumber < paginatedFileData.CurrentPage - 2
                || pageNumber > paginatedFileData.CurrentPage + 2)
            {
                continue;
            }

            if (pageNumber == paginatedFileData.CurrentPage - 2
                || pageNumber == paginatedFileData.CurrentPage + 2)
            {
                paginationLinks.Add(new PaginationItemViewModel()
                {
                    Ellipsis = true
                });
            }

            if (pageNumber > paginatedFileData.CurrentPage - 2
                && pageNumber < paginatedFileData.CurrentPage + 2)
            {
                paginationLinks.Add(new PaginationItemViewModel()
                {
                    Number = pageNumber.ToString(),
                    Current = paginatedFileData.CurrentPage == pageNumber,
                    Href = pageLinkGenerator(pageNumber)
                });
            }
        }

        paginationLinks.Add(new PaginationItemViewModel()
        {
            Number = paginatedFileData.MaximumPage.ToString(),
            Current = paginatedFileData.CurrentPage == paginatedFileData.MaximumPage,
            Href = pageLinkGenerator(paginatedFileData.MaximumPage)
        });
        
        var paginationDetails = new PaginationViewModel()
        {
            Items = paginationLinks
        };
        
        if (paginatedFileData.CurrentPage > 1)
        {
            paginationDetails.Previous = new PaginationLinkViewModel()
            {
                Href = pageLinkGenerator(paginatedFileData.CurrentPage - 1)
            };
        }
    
        if (paginatedFileData.CurrentPage < paginatedFileData.MaximumPage)
        {
            paginationDetails.Next = new PaginationLinkViewModel()
            {
                Href = pageLinkGenerator(paginatedFileData.CurrentPage + 1)
            };
        }

        return paginationDetails;
    }
}
