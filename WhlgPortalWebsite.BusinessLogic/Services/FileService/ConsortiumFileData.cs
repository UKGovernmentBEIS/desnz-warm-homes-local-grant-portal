using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services.FileService;

public class ConsortiumFileData(
    string code,
    int month,
    int year,
    DateTime lastUpdated,
    DateTime? lastDownloaded)
    : FileData(code, month, year, lastUpdated, lastDownloaded)
{
    public override string Name => ConsortiumData.ConsortiumNamesByConsortiumCode[Code];
}