namespace HerPortal.BusinessLogic.Models;

public class AuditDownload
{
    public string CustodianCode { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string UserEmail { get; set; }
    public DateTime Timestamp { get; set; }
}
