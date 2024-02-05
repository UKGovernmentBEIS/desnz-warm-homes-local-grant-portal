using HerPortal.BusinessLogic.Models;

namespace HerPortal.BusinessLogic.Services.CsvFileService;

public class LocalAuthorityCsvFileData : CsvFileData
{
    public override string Name => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[Code];

    public LocalAuthorityCsvFileData
    (
        string code,
        int month,
        int year,
        DateTime lastUpdated,
        DateTime? lastDownloaded
    ) : base(code, month, year, lastUpdated, lastDownloaded)
    { }
}