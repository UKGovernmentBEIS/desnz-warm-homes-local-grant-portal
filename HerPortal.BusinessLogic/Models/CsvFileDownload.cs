namespace HerPortal.BusinessLogic.Models;

public class CsvFileDownload
{
    public int CsvFileId { get; set; }
    public TrackedCsvFile CsvFile { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime Timestamp { get; set; }
}
