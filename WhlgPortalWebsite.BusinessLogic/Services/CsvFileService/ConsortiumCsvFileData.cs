using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;

public class ConsortiumCsvFileData : CsvFileData
{
    public override string Name => ConsortiumData.ConsortiumNamesByConsortiumCode[Code];

    public ConsortiumCsvFileData
    (
        string code,
        int month,
        int year,
        DateTime lastUpdated,
        DateTime? lastDownloaded
    ) : base(code, month, year, lastUpdated, lastDownloaded)
    { }
}