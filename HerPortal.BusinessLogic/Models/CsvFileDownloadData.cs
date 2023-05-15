namespace HerPortal.BusinessLogic.Models;

public class CsvFileDownloadData
{
    public string CustodianCode { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public List<CsvFileDownload> Downloads { get; set; }
    public CsvFileDownload LastDownload => Downloads.MaxBy(d => d.DateTime);
    public DateTime? LastDownloaded => LastDownload?.DateTime;
    public User LastDownloadedBy => LastDownload?.User;
}
