using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhlgPortalWebsite.BusinessLogic;
using WhlgPortalWebsite.BusinessLogic.ExternalServices.EmailSending;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using Tests.Builders;

namespace Tests.BusinessLogic.Services;

[TestFixture]
public class RegularJobsServiceTests
{
    private Mock<ILogger<RegularJobsService>> mockLogger;
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private Mock<IEmailSender> mockEmailSender;
    private Mock<IFileService> mockCsvFileService;
    private RegularJobsService underTest;
    
    private const string EmailAddress = "test@example.com";
    
    [SetUp]
    public void Setup()
    {
        mockLogger = new Mock<ILogger<RegularJobsService>>();
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockEmailSender = new Mock<IEmailSender>();
        mockCsvFileService = new Mock<IFileService>();
        
        underTest = new RegularJobsService(mockDataAccessProvider.Object, mockEmailSender.Object, mockCsvFileService.Object, mockLogger.Object);
    }
    
    [Test]
    public async Task SendReminderEmailsAsync_WhenThereAreNoFiles_DoesntSendEmails()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder(EmailAddress).Build()
        };
        mockDataAccessProvider.Setup(dap => dap.GetAllActiveUsersAsync()).ReturnsAsync(users);
        
        // Act
        await underTest.SendReminderEmailsAsync();
        
        // Assert
        mockEmailSender.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task SendReminderEmailsAsync_WhenThereAreNoUpdates_DoesntSendEmails()
    {
        // Arrange
        var files = new List<LocalAuthorityFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), new DateTime(2023, 02, 01)),
        };
        
        var users = new List<User>
        {
            new UserBuilder(EmailAddress)
                .WithLocalAuthorities(new List<LocalAuthority>
                {
                    new()
                    {
                        Id = 1,
                        CustodianCode = "114"
                    }
                })
                .Build()
        };
        mockDataAccessProvider.Setup(dap => dap.GetAllActiveUsersAsync()).ReturnsAsync(users);
        mockCsvFileService.Setup(cfg => cfg.GetFileDataForUserAsync(users[0].EmailAddress)).ReturnsAsync(files);
        
        // Act
        await underTest.SendReminderEmailsAsync();
        
        // Assert
        mockEmailSender.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task SendReminderEmailsAsync_WhenThereAreUpdates_SendsEmails()
    {
        // Arrange
        var user1Files = new List<LocalAuthorityFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), new DateTime(2023, 01, 30)),
        };
        var user2Files = new List<LocalAuthorityFileData>()
        {
            new("910", 1, 2023, new DateTime(2023, 1, 31), new DateTime(2023, 02, 01)),
        };
        
        var users = new List<User>
        {
            new UserBuilder(EmailAddress)
                .WithLocalAuthorities(new List<LocalAuthority>
                {
                    new()
                    {
                        Id = 1,
                        CustodianCode = "114"
                    }
                })
                .WithEmail("user1@example.com")
                .Build(),
            new UserBuilder(EmailAddress)
                .WithLocalAuthorities(new List<LocalAuthority>
                {
                    new()
                    {
                        Id = 2,
                        CustodianCode = "910"
                    }
                })
                .WithEmail("user2@example.com")
                .Build()
        };
        mockDataAccessProvider.Setup(dap => dap.GetAllActiveUsersAsync()).ReturnsAsync(users);
        mockCsvFileService.Setup(cfg => cfg.GetFileDataForUserAsync(users[0].EmailAddress)).ReturnsAsync(user1Files);
        mockCsvFileService.Setup(cfg => cfg.GetFileDataForUserAsync(users[1].EmailAddress)).ReturnsAsync(user2Files);
        
        // Act
        await underTest.SendReminderEmailsAsync();
        
        // Assert
        mockEmailSender.Verify(es => es.SendNewReferralReminderEmail("user1@example.com"));
        mockEmailSender.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task SendReminderEmailsAsync_WhenThereAreErrors_ContinuesSendingEmails()
    {
        // Arrange
        var user1Files = new List<LocalAuthorityFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), new DateTime(2023, 01, 30)),
        };
        var user2Files = new List<LocalAuthorityFileData>()
        {
            new("910", 1, 2023, new DateTime(2023, 1, 31), new DateTime(2023, 01, 30)),
        };
        
        var users = new List<User>
        {
            new UserBuilder(EmailAddress)
                .WithLocalAuthorities(new List<LocalAuthority>
                {
                    new()
                    {
                        Id = 1,
                        CustodianCode = "114"
                    }
                })
                .WithEmail("user1@example.com")
                .Build(),
            new UserBuilder(EmailAddress)
                .WithLocalAuthorities(new List<LocalAuthority>
                {
                    new()
                    {
                        Id = 2,
                        CustodianCode = "910"
                    }
                })
                .WithEmail("user2@example.com")
                .Build()
        };
        mockDataAccessProvider.Setup(dap => dap.GetAllActiveUsersAsync()).ReturnsAsync(users);
        mockCsvFileService.Setup(cfg => cfg.GetFileDataForUserAsync(users[0].EmailAddress)).ReturnsAsync(user1Files);
        mockCsvFileService.Setup(cfg => cfg.GetFileDataForUserAsync(users[1].EmailAddress)).ReturnsAsync(user2Files);
        mockEmailSender.Setup(es => es.SendNewReferralReminderEmail(users[0].EmailAddress)).Throws(new EmailSenderException(EmailSenderExceptionType.Other));
        
        // Act
        await underTest.SendReminderEmailsAsync();
        
        // Assert
        mockEmailSender.Verify(es => es.SendNewReferralReminderEmail(users[0].EmailAddress));
        mockEmailSender.Verify(es => es.SendNewReferralReminderEmail(users[1].EmailAddress));
        mockEmailSender.VerifyNoOtherCalls();
    }
}
