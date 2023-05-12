using System;
using System.Collections.Generic;
using System.Linq;
using HerPortal.BusinessLogic.Models;
using HerPortal.ExternalServices.CsvFiles;
using HerPublicWebsite.BusinessLogic.Models;

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
            MonthAndYear = new DateOnly(csvFileData.Year, csvFileData.Month, 1).ToString("MMMM yyyy");
            LocalAuthorityName = LocalAuthorityData
                .LocalAuthorityDetailsByCustodianCode[csvFileData.CustodianCode]
                .Name;
            LastUpdated = csvFileData.LastUpdated.ToString("dd/MM/yy");
            HasNewUpdates = csvFileData.HasUpdatedSinceLastDownload;
            HasApplications = csvFileData.HasApplications;
        }
    }
    
    public bool ShouldShowBanner { get; }
    public IEnumerable<CsvFile> CsvFiles { get; }

    public HomepageViewModel(User user, IEnumerable<CsvFileData> csvFiles)
    {
        ShouldShowBanner = !user.HasLoggedIn;
        CsvFiles = csvFiles.Select(cf => new CsvFile(cf));
    }
}
