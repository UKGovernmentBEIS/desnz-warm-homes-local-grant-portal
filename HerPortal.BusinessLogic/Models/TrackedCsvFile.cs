namespace HerPortal.BusinessLogic.Models;

public class TrackedCsvFile
{
    public int Id { get; set; }
    public string CustodianCode { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    
    public List<CsvFileDownload> Downloads { get; set; }

    public DateTime? LastDownloaded => Downloads
        .OrderByDescending(d => d.Timestamp)
        .Select(d => d.Timestamp)
        .FirstOrDefault();
}
