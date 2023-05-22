namespace HerPortal.ExternalServices.EmailSending
{
    public class GovUkNotifyConfiguration
    {
        public const string ConfigSection = "GovUkNotify";
        
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public ReferralReminderConfiguration ReferralReminderTemplate { get; set; }
    }

    public class ReferralReminderConfiguration
    {
        public string Id { get; set; }
    }
}
