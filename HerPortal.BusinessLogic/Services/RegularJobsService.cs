using HerPortal.ExternalServices.EmailSending;

namespace HerPortal.Data.Services;

public class RegularJobsService
{
    private readonly IDataAccessProvider dataProvider;
    private readonly IEmailSender emailSender;

    public RegularJobsService
    (
        IDataAccessProvider dataProvider,
        IEmailSender emailSender
    ) {
        this.dataProvider = dataProvider;
        this.emailSender = emailSender;
    }

    public async Task SendReminderEmailsAsync()
    {
        
    }
}