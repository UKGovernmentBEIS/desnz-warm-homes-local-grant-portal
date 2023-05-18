using System;
using System.Collections.Generic;
using System.Linq;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using HerPortal.BusinessLogic.Models;
using HerPortal.ExternalServices.CsvFiles;

namespace HerPortal.Models;

public class HomepageViewModel
{
    public class CsvFile
    {
        public string MonthAndYear { get; }
        public string LocalAuthorityName { get; }
        public string LastUpdated { get; }
        public bool HasNewUpdates { get; }
        public bool HasApplications { get; }

        public CsvFile(CsvFileData csvFileData)
        {
            var localAuthorityExists = LocalAuthorityData
                .LocalAuthorityNamesByCustodianCode
                .TryGetValue(csvFileData.CustodianCode, out var laName);
            
            if (!localAuthorityExists)
            {
                throw new ArgumentOutOfRangeException(nameof(csvFileData.CustodianCode), csvFileData.CustodianCode,
                    "The given custodian code is not known.");
            }

            MonthAndYear = new DateOnly(csvFileData.Year, csvFileData.Month, 1).ToString("MMMM yyyy");
            LocalAuthorityName = laName;
            LastUpdated = csvFileData.LastUpdated.ToString("dd/MM/yy");
            HasNewUpdates = csvFileData.HasUpdatedSinceLastDownload;
            HasApplications = csvFileData.HasApplications;
        }
    }
    
    public bool ShouldShowBanner { get; }
    public List<string> CustodianCodes { get; }
    public Dictionary<string, LabelViewModel> LocalAuthorityCheckboxLabels { get; }
    public IEnumerable<CsvFile> CsvFiles { get; }

    public HomepageViewModel(User user, IEnumerable<CsvFileData> csvFiles)
    {
        ShouldShowBanner = !user.HasLoggedIn;
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
        );
        CsvFiles = csvFiles.Select(cf => new CsvFile(cf));
    }
}
