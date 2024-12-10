namespace WhlgPortalWebsite.BusinessLogic.ExternalServices.EmailSending
{
    public interface IEmailSender
    {
        public void SendNewReferralReminderEmail(string emailAddress);
    }
}