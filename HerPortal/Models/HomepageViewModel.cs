using System;
using System.Collections.Generic;
using System.Linq;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services.CsvFileService;
using Microsoft.AspNetCore.Mvc.Routing;

namespace HerPortal.Models;

public class HomepageViewModel
{
    public class CsvFile
    {
        public string CustodianCode { get; }
        public int Year { get; }
        public int Month { get; }
        public string MonthAndYearText => new DateOnly(Year, Month, 1).ToString("MMMM yyyy");
        public string Name { get; }
        public string LastUpdatedText { get; }
        public bool HasNewUpdates { get; }
        public string DownloadLink { get; }

        public CsvFile(CsvFileData csvFileData, string downloadLink)
        {
            switch (csvFileData)
            {
                case LocalAuthorityCsvFileData:
                    if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(csvFileData.Code))
                    {
                        throw new ArgumentOutOfRangeException(nameof(csvFileData.Code), csvFileData.Code,
                            "The given custodian code is not known.");
                    }
                    break;
                case ConsortiumCsvFileData:
                    if (!ConsortiumData.ConsortiumNamesByConsortiumCode.ContainsKey(csvFileData.Code))
                    {
                        throw new ArgumentOutOfRangeException(nameof(csvFileData.Code), csvFileData.Code,
                            "The given consortium code is not known.");
                    }
                    break;
            }
            
            CustodianCode = csvFileData.Code;
            Year = csvFileData.Year;
            Month = csvFileData.Month;
            LastUpdatedText = csvFileData.LastUpdated.ToString("dd/MM/yy");
            HasNewUpdates = csvFileData.HasUpdatedSinceLastDownload;
            Name = csvFileData is ConsortiumCsvFileData ? $"{csvFileData.Name} (Consortium)" : csvFileData.Name;
            DownloadLink = downloadLink;
        }
    }
    
    public bool ShouldShowBanner { get; }
    public bool ShouldShowFilters { get; }
    public bool UserHasNewUpdates { get; }
    public List<string> Codes { get; }
    public Dictionary<string, LabelViewModel> LocalAuthorityCheckboxLabels { get; }
    public IEnumerable<CsvFile> CsvFiles { get; }
    public int CurrentPage { get; }
    public string[] PageUrls { get; }

    public HomepageViewModel(
        User user,
        PaginatedFileData paginatedFileData,
        Func<int, string> pageLinkGenerator,
        Func<CsvFileData, string> downloadLinkGenerator
    )
    {
        var custodianCodes = user.GetAdministeredCustodianCodes().ToList();
        var consortiumCodes = user.GetAdministeredConsortiumCodes().ToList();
        var checkboxLabels = custodianCodes
            .Select(custodianCode => new KeyValuePair<string, LabelViewModel>
                (
                    custodianCode,
                    new LabelViewModel
                    {
                        Text = LocalAuthorityData.LocalAuthorityNamesByCustodianCode[custodianCode]
                    }
                )
            )
            .ToList();

        checkboxLabels.AddRange(consortiumCodes.Select(consortiumCode => new KeyValuePair<string, LabelViewModel>(
            consortiumCode,
            new LabelViewModel
            {
                Text = $"{ConsortiumData.ConsortiumNamesByConsortiumCode[consortiumCode]} (Consortium)"
            }
        )));

        ShouldShowBanner = !user.HasLoggedIn;
        ShouldShowFilters = custodianCodes.Count >= 2;
        Codes = new List<string>();
        Codes.AddRange(custodianCodes);
        Codes.AddRange(consortiumCodes);
        LocalAuthorityCheckboxLabels = new Dictionary<string, LabelViewModel>(checkboxLabels
            .OrderBy(kvp => kvp.Value.Text)
        );
        CsvFiles = paginatedFileData.FileData.Select(cf => new CsvFile(cf, downloadLinkGenerator(cf)));

        UserHasNewUpdates = paginatedFileData.UserHasUndownloadedFiles;

        CurrentPage = paginatedFileData.CurrentPage;
        PageUrls = Enumerable.Range(1, paginatedFileData.MaximumPage).Select(pageLinkGenerator).ToArray();
    }
}
