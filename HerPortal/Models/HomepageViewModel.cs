using System.Collections.Generic;
using HerPortal.BusinessLogic.Models;

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

        public CsvFile
        (
            string monthAndYear,
            string localAuthorityName,
            string lastUpdated,
            bool hasNewUpdates,
            bool hasApplications
        ) {
            MonthAndYear = monthAndYear;
            LocalAuthorityName = localAuthorityName;
            LastUpdated = lastUpdated;
            HasNewUpdates = hasNewUpdates;
            HasApplications = hasApplications;
        }
    }
    
    public bool ShouldShowBanner { get; }
    public IEnumerable<CsvFile> CsvFiles { get; }

    public HomepageViewModel(User user, IEnumerable<CsvFile> csvFiles)
    {
        ShouldShowBanner = !user.HasLoggedIn;
        CsvFiles = new List<CsvFile>(csvFiles);
    }
}
