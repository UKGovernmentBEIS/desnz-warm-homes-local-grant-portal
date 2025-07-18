﻿using Microsoft.Extensions.Logging;
using WhlgPortalWebsite.BusinessLogic.ExternalServices.EmailSending;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;

namespace WhlgPortalWebsite.BusinessLogic.Services;

public interface IReminderEmailsService
{
    Task SendReminderEmailsAsync();
}

public class ReminderEmailsService(
    IUserService userService,
    IEmailSender emailSender,
    IFileRetrievalService fileRetrievalService,
    ILogger<RegularJobsService> logger) : IReminderEmailsService
{
    private readonly ILogger logger = logger;

    public async Task SendReminderEmailsAsync()
    {
        logger.LogInformation("Sending reminder emails");

        var activeUsers = await userService.GetAllActiveDeliveryPartnersAsync();
        foreach (var user in activeUsers)
        {
            IEnumerable<FileData> userCsvFiles;

            try
            {
                userCsvFiles = await fileRetrievalService.GetFileDataForUserAsync(user.EmailAddress);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error encountered while attempting to read files from S3");
                throw;
            }

            var hasUpdates = userCsvFiles.Any(cf => cf.HasUpdatedSinceLastDownload);

            if (!hasUpdates) continue;

            try
            {
                emailSender.SendNewReferralReminderEmail(user.EmailAddress);
            }
            catch (EmailSenderException ex)
            {
                // We log a warning here and do not re-throw, as we don't want this error
                //   to prevent the emails being sent to the other users.
                // If an email fails to be sent, this will have to be caught by
                //   checking the logs.
                logger.LogWarning
                (
                    ex,
                    "Error encountered while attempting to send reminder email to {EmailAddress}",
                    user.EmailAddress
                );
            }
        }
    }
}