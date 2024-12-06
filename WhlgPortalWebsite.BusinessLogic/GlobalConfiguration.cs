namespace HerPortal.BusinessLogic;

public class GlobalConfiguration
{
    public const string ConfigSection = "Global";
    
    public string AppBaseUrl { get; set; }
    public string ReferralReminderCrontab { get; set; }
}
