using HerPortal.BusinessLogic.Models;

namespace HerPortal.BusinessLogic.Services.CsvFileService;

public class ConsortiumCsvFileData : AbstractCsvFileData
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