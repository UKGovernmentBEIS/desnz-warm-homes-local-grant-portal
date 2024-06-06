using System.Collections.Generic;
using System.Linq;
using HerPortal.BusinessLogic.Models;
using HerPortal.ManagementShell;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.ManagementShell;

public class MigrateAdminsCommandTests
{
    private Mock<IDatabaseOperation> mockDatabaseOperation;
    private Mock<IOutputProvider> mockOutputProvider;
    private CommandHandler underTest;

    [SetUp]
    public void Setup()
    {
        mockOutputProvider = new Mock<IOutputProvider>();
        mockDatabaseOperation = new Mock<IDatabaseOperation>();
        var adminAction = new AdminAction(mockDatabaseOperation.Object);
        underTest = new CommandHandler(adminAction, mockOutputProvider.Object);
    }

    // Cheshire East C_0008 contains only two LAs:
    // - Cheshire East 660
    // - Cheshire West and Chester 665

    [Test]
    public void MigrateAdmins_IfOwnsAllLas_RemovesLasAndAddConsortia()
    {
        // Arrange
        var (user, expectedLasToRemove, expectedConsortiaToAdd) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = new List<string> { "660", "665" },
            ExpectedCustodianCodesToRemove = new List<string> { "660", "665" },
            ExpectedConsortiumCodesToAdd = new List<string> { "C_0008" }
        });

        // Act
        underTest.MigrateAdmins();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(db => db.GetLas(new List<string> { "660", "665" }));
        mockDatabaseOperation.Verify(db => db.GetConsortia(new List<string> { "C_0008" }));
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(user, expectedConsortiaToAdd, expectedLasToRemove));

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void MigrateAdmins_IfOwnsNotEnoughLas_DoesNothing()
    {
        // Arrange
        var (user, _, _) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = new List<string> { "660" }
        });

        // Act
        underTest.MigrateAdmins();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(
                user, It.IsAny<List<Consortium>>(), It.IsAny<List<LocalAuthority>>()),
            Times.Never);

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void MigrateAdmins_IfOwnsAllLasInTwoConsortia_AddsBoth()
    {
        // Arrange
        var (user, expectedLasToRemove, expectedConsortiaToAdd) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = new List<string> { "660", "665", "835", "840" },
            ExpectedCustodianCodesToRemove = new List<string> { "660", "665", "835", "840" },
            ExpectedConsortiumCodesToAdd = new List<string> { "C_0008", "C_0010" }
        });

        // Act
        underTest.MigrateAdmins();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(db => db.GetLas(new List<string> { "660", "665", "835", "840" }));
        mockDatabaseOperation.Verify(db => db.GetConsortia(new List<string> { "C_0008", "C_0010" }));
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(user, expectedConsortiaToAdd, expectedLasToRemove));

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void MigrateAdmins_IfOwnsAllInAConsortiaButNotEnoughInAnother_AddsOneConsortia()
    {
        // Arrange
        var (user, expectedLasToRemove, expectedConsortiaToAdd) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = new List<string> { "660", "665", "835" },
            ExpectedCustodianCodesToRemove = new List<string> { "660", "665" },
            ExpectedConsortiumCodesToAdd = new List<string> { "C_0008" }
        });

        // Act
        underTest.MigrateAdmins();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(db => db.GetLas(new List<string> { "660", "665" }));
        mockDatabaseOperation.Verify(db => db.GetConsortia(new List<string> { "C_0008" }));
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(user, expectedConsortiaToAdd, expectedLasToRemove));

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void MigrateAdmins_IfOwnsConsortia_DoesNothing()
    {
        // Arrange
        var (user, _, _) = SetupUser(new UserTestSetup
        {
            UserConsortiumCodes = new List<string> { "C_0008" }
        });

        // Act
        underTest.MigrateAdmins();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(
                user, It.IsAny<List<Consortium>>(), It.IsAny<List<LocalAuthority>>()),
            Times.Never);

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    private (User, List<LocalAuthority>, List<Consortium>) SetupUser(UserTestSetup userTestSetup)
    {
        var localAuthorities = userTestSetup.UserCustodianCodes
            .Select((custodianCode, i) => new LocalAuthority
            {
                Id = i,
                CustodianCode = custodianCode
            })
            .ToList();
        var consortia = userTestSetup.UserConsortiumCodes
            .Select((consortiumCode, i) => new Consortium
            {
                Id = i,
                ConsortiumCode = consortiumCode
            })
            .ToList();
        var expectedConsortiaToAdd = userTestSetup.ExpectedConsortiumCodesToAdd
            .Select((consortiumCode, i) => new Consortium
            {
                Id = i + userTestSetup.UserConsortiumCodes.Count,
                ConsortiumCode = consortiumCode
            })
            .ToList();
        var expectedLasToRemove = userTestSetup.ExpectedCustodianCodesToRemove.Select(custodianCode =>
            localAuthorities.Single(la => la.CustodianCode == custodianCode)).ToList();

        mockOutputProvider.Setup(op => op.Confirm("Okay to proceed? (Y/N)")).Returns(true);

        var user = new UserBuilder("user@example.com")
            .WithLocalAuthorities(localAuthorities)
            .WithConsortia(consortia)
            .Build();
        mockDatabaseOperation
            .Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia())
            .Returns(new List<User> { user });
        mockDatabaseOperation
            .Setup(db => db.GetConsortia(userTestSetup.ExpectedConsortiumCodesToAdd))
            .Returns(expectedConsortiaToAdd);
        mockDatabaseOperation
            .Setup(db => db.GetLas(userTestSetup.ExpectedCustodianCodesToRemove))
            .Returns(expectedLasToRemove);

        return (user, expectedLasToRemove, expectedConsortiaToAdd);
    }

    private struct UserTestSetup
    {
        public IEnumerable<string> UserCustodianCodes = new List<string>();
        public IReadOnlyCollection<string> UserConsortiumCodes = new List<string>();
        public IReadOnlyCollection<string> ExpectedCustodianCodesToRemove = new List<string>();
        public IReadOnlyCollection<string> ExpectedConsortiumCodesToAdd = new List<string>();

        public UserTestSetup()
        {
        }
    }
}