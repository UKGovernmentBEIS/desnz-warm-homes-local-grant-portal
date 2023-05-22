namespace HerPortal.ExternalServices.EmailSending
{
    public interface IEmailSender
    {
        public void SendNewReferralReminderEmail(string emailAddress);
    }
}