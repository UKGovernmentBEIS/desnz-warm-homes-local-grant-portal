namespace HerPortal.BusinessLogic.Models;

public class CsvFileDownload
{
    public string CustodianCode { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime? LastDownloaded { get; set; }
}
