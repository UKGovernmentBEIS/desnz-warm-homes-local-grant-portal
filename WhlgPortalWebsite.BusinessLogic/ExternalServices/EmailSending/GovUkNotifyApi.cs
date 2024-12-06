using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models.Responses;

namespace HerPortal.BusinessLogic.ExternalServices.EmailSending
{
    public class GovUkNotifyApi: IEmailSender
    {
        private readonly NotificationClient client;
        private readonly GlobalConfiguration globalConfig;
        private readonly GovUkNotifyConfiguration govUkNotifyConfig;
        private readonly ILogger<GovUkNotifyApi> logger;
        
        public GovUkNotifyApi
        (
            IOptions<GlobalConfiguration> globalConfig,
            IOptions<GovUkNotifyConfiguration> govUkNotifyConfig,
            ILogger<GovUkNotifyApi> logger
        )
        {
            this.globalConfig = globalConfig.Value;
            this.govUkNotifyConfig = govUkNotifyConfig.Value;
            
            client = new NotificationClient(this.govUkNotifyConfig.ApiKey);
            this.logger = logger;
        }

        private EmailNotificationResponse SendEmail(GovUkNotifyEmailModel emailModel)
        {
            try
            {
                var response = client.SendEmail(
                    emailModel.EmailAddress,
                    emailModel.TemplateId,
                    emailModel.Personalisation,
                    emailModel.Reference,
                    emailModel.EmailReplyToId);
                return response;
            }
            catch (NotifyClientException e)
            {
                if (e.Message.Contains("Not a valid email address"))
                {
                    throw new EmailSenderException(EmailSenderExceptionType.InvalidEmailAddress);
                }

                logger.LogError("GOV.UK Notify returned an error: " + e.Message);
                throw new EmailSenderException(EmailSenderExceptionType.Other);
            }
        }

        public void SendNewReferralReminderEmail(string emailAddress)
        {
            var template = govUkNotifyConfig.ReferralReminderTemplate;
            var personalisation = new Dictionary<string, dynamic>
            {
                { template.AppBaseUrlPlaceholder, globalConfig.AppBaseUrl }
            };
            var emailModel = new GovUkNotifyEmailModel
            {
                EmailAddress = emailAddress,
                TemplateId = template.Id,
                Personalisation = personalisation
            };
            var response = SendEmail(emailModel);
        }
    }

    internal class GovUkNotifyEmailModel
    {
        public string EmailAddress { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, dynamic> Personalisation { get; set; }
        public string Reference { get; set; }
        public string EmailReplyToId { get; set; }
    }
}
