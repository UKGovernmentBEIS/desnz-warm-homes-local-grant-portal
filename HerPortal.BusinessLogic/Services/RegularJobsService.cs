using HerPortal.ExternalServices.CsvFiles;
using HerPortal.ExternalServices.EmailSending;

namespace HerPortal.Data.Services;

public class RegularJobsService
{
    private readonly IDataAccessProvider dataProvider;
    private readonly IEmailSender emailSender;
    private readonly ICsvFileGetter csvFileGetter;

    public RegularJobsService
    (
        IDataAccessProvider dataProvider,
        IEmailSender emailSender,
        ICsvFileGetter csvFileGetter
    ) {
        this.dataProvider = dataProvider;
        this.emailSender = emailSender;
        this.csvFileGetter = csvFileGetter;
    }

    public async Task SendReminderEmailsAsync()
    {
        
    }
}