namespace WhlgPortalWebsite.BusinessLogic.Services;

public class RegularJobsService(
    ReminderEmailsService reminderEmailsService)
{
    public async Task SendReminderEmailsAsync()
    {
        await reminderEmailsService.SendReminderEmailsAsync();
    }
}