using System.Collections.Generic;
using HerPortal.BusinessLogic.Models;
using HerPortal.ExternalServices.CsvFiles;

namespace HerPortal.Models;

public class HomepageViewModel
{
    public bool ShouldShowBanner { get; }
    public IEnumerable<CsvFileData> CsvFiles { get; }

    public HomepageViewModel(User user, IEnumerable<CsvFileData> csvFiles)
    {
        ShouldShowBanner = !user.HasLoggedIn;
        CsvFiles = new List<CsvFileData>(csvFiles);
    }
}
