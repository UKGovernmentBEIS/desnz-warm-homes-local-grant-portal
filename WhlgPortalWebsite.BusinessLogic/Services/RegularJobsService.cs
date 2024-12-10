﻿using HerPortal.BusinessLogic.ExternalServices.EmailSending;
using HerPortal.BusinessLogic.Services.CsvFileService;
using Microsoft.Extensions.Logging;

namespace HerPortal.BusinessLogic.Services;

public class RegularJobsService
{
    private readonly IDataAccessProvider dataProvider;
    private readonly IEmailSender emailSender;
    private readonly ICsvFileService csvFileService;

    private readonly ILogger logger;

    public RegularJobsService
    (
        IDataAccessProvider dataProvider,
        IEmailSender emailSender,
        ICsvFileService csvFileService,
        ILogger<RegularJobsService> logger
    ) {
        this.dataProvider = dataProvider;
        this.emailSender = emailSender;
        this.csvFileService = csvFileService;

        this.logger = logger;
    }

    public async Task SendReminderEmailsAsync()
    {
        logger.LogInformation("Sending reminder emails");
        
        var activeUsers = await dataProvider.GetAllActiveUsersAsync();
        foreach (var user in activeUsers)
        {
            IEnumerable<CsvFileData> userCsvFiles;

            try
            {
                userCsvFiles = await csvFileService.GetFileDataForUserAsync(user.EmailAddress);
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
