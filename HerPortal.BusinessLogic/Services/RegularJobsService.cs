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
        var activeUsers = await dataProvider.GetAllActiveUsersAsync();
        foreach (var user in activeUsers)
        {
            var userCsvFiles = await csvFileGetter.GetByCustodianCodesAsync
            (
                user.LocalAuthorities.Select(la => la.CustodianCode),
                user.Id
            );
            var hasUpdates = userCsvFiles.Any(cf => cf.HasUpdatedSinceLastDownload);
            if (hasUpdates)
            {
                emailSender.SendNewReferralReminderEmail(user.EmailAddress);
            }
        }
    }
}