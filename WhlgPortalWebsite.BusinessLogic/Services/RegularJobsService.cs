namespace WhlgPortalWebsite.BusinessLogic.Services;

public class RegularJobsService(
    IReminderEmailsService reminderEmailsService)
{
    public async Task SendReminderEmailsAsync()
    {
        await reminderEmailsService.SendReminderEmailsAsync();
    }
}