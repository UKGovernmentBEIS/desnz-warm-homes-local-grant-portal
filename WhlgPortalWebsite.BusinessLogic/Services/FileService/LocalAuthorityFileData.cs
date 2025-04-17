using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services.FileService;

public class LocalAuthorityFileData : FileData
{
    public override string Name => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[Code];

    public LocalAuthorityFileData
    (
        string code,
        int month,
        int year,
        DateTime lastUpdated,
        DateTime? lastDownloaded
    ) : base(code, month, year, lastUpdated, lastDownloaded)
    { }
}