using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HerPortal.BusinessLogic.ExternalServices.CsvFiles;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services;
using HerPortal.Data;
using HerPortal.ExternalServices.EmailSending;
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
    private Mock<ICsvFileGetter> mockCsvFileGetter;
    private RegularJobsService underTest;
    
    private const string EmailAddress = "test@example.com";
    
    [SetUp]
    public void Setup()
    {
        mockLogger = new Mock<ILogger<RegularJobsService>>();
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockEmailSender = new Mock<IEmailSender>();
        mockCsvFileGetter = new Mock<ICsvFileGetter>();
        
        underTest = new RegularJobsService(mockDataAccessProvider.Object, mockEmailSender.Object, mockCsvFileGetter.Object, mockLogger.Object);
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
        var files = new List<CsvFileData>()
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
        mockCsvFileGetter.Setup(cfg => cfg.GetByCustodianCodesAsync(new[] { "114" }, users[0].Id )).ReturnsAsync(files);
        
        // Act
        await underTest.SendReminderEmailsAsync();
        
        // Assert
        mockEmailSender.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task SendReminderEmailsAsync_WhenThereAreUpdates_SendsEmails()
    {
        // Arrange
        var user1Files = new List<CsvFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), new DateTime(2023, 01, 30)),
        };
        var user2Files = new List<CsvFileData>()
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
        mockCsvFileGetter.Setup(cfg => cfg.GetByCustodianCodesAsync(new[] { "114" }, users[0].Id )).ReturnsAsync(user1Files);
        mockCsvFileGetter.Setup(cfg => cfg.GetByCustodianCodesAsync(new[] { "910" }, users[1].Id )).ReturnsAsync(user2Files);
        
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
        var user1Files = new List<CsvFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 1, 31), new DateTime(2023, 01, 30)),
        };
        var user2Files = new List<CsvFileData>()
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
        mockCsvFileGetter.Setup(cfg => cfg.GetByCustodianCodesAsync(new[] { "114" }, users[0].Id )).ReturnsAsync(user1Files);
        mockCsvFileGetter.Setup(cfg => cfg.GetByCustodianCodesAsync(new[] { "910" }, users[1].Id )).ReturnsAsync(user2Files);
        mockEmailSender.Setup(es => es.SendNewReferralReminderEmail("user1@example.com")).Throws(new EmailSenderException(EmailSenderExceptionType.Other));
        
        // Act
        await underTest.SendReminderEmailsAsync();
        
        // Assert
        mockEmailSender.Verify(es => es.SendNewReferralReminderEmail("user1@example.com"));
        mockEmailSender.Verify(es => es.SendNewReferralReminderEmail("user2@example.com"));
        mockEmailSender.VerifyNoOtherCalls();
    }
}
