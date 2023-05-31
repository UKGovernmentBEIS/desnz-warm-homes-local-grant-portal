using System;
using System.Collections.Generic;
using System.Linq;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using HerPortal.BusinessLogic.Models;
using CsvFileData = HerPortal.ExternalServices.CsvFiles.CsvFileData;

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
        public bool HasApplications { get; }

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
            HasApplications = csvFileData.HasApplications;
        }
    }
    
    public bool ShouldShowBanner { get; }
    public bool ShouldShowFilters { get; }
    public bool UserHasNewUpdates { get; }
    public List<string> CustodianCodes { get; }
    public Dictionary<string, LabelViewModel> LocalAuthorityCheckboxLabels { get; }
    public IEnumerable<CsvFile> CsvFiles { get; }

    public HomepageViewModel(User user, IEnumerable<CsvFileData> csvFiles, bool userHasNewUpdates)
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
        CsvFiles = csvFiles.Select(cf => new CsvFile(cf));

        UserHasNewUpdates = userHasNewUpdates;
    }
}
