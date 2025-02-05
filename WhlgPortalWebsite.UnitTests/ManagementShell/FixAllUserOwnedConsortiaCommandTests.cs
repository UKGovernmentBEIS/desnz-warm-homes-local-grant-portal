using System.Collections.Generic;
using System.Linq;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.ManagementShell;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.ManagementShell;

public class FixAllUserOwnedConsortiaCommandTests
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

    [Test]
    public void FixAllUserOwnedConsortia_IfOwnsAllLas_RemovesLasAndAddConsortia()
    {
        // Arrange
        var (consortiumCodes, custodianCodes) = GetExampleConsortiumCodesWithCustodianCodes().First();

        var (user, expectedLasToRemove, expectedConsortiaToAdd) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = custodianCodes,
            ExpectedCustodianCodesToRemove = custodianCodes,
            ExpectedConsortiumCodesToAdd = new List<string> { consortiumCodes }
        });

        // Act
        underTest.FixAllUserOwnedConsortia();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(db => db.GetLas(custodianCodes));
        mockDatabaseOperation.Verify(db => db.GetConsortia(new List<string> { consortiumCodes }));
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(user, expectedConsortiaToAdd, expectedLasToRemove));

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void FixAllUserOwnedConsortia_IfOwnsNotEnoughLas_DoesNothing()
    {
        // Arrange
        var (_, custodianCodes) = GetExampleConsortiumCodesWithCustodianCodes().First();
        var userCustodianCodes = custodianCodes.Take(1).ToList();

        var (user, _, _) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = userCustodianCodes
        });

        // Act
        underTest.FixAllUserOwnedConsortia();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(
                user, It.IsAny<List<Consortium>>(), It.IsAny<List<LocalAuthority>>()),
            Times.Never);

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void FixAllUserOwnedConsortia_IfOwnsAllLasInTwoConsortia_AddsBoth()
    {
        // Arrange
        var consortiumAndLasInfo = GetExampleConsortiumCodesWithCustodianCodes().Take(2).ToList();
        var (consortiumCode1, custodianCodes1) = consortiumAndLasInfo[0];
        var (consortiumCode2, custodianCodes2) = consortiumAndLasInfo[1];
        var userCustodianCodes = custodianCodes1.Concat(custodianCodes2).ToList();
        var expectedConsortiumCodesToAdd = new List<string> { consortiumCode1, consortiumCode2 };

        var (user, expectedLasToRemove, expectedConsortiaToAdd) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = userCustodianCodes,
            ExpectedCustodianCodesToRemove = custodianCodes1.Concat(custodianCodes2).ToList(),
            ExpectedConsortiumCodesToAdd = expectedConsortiumCodesToAdd
        });

        // Act
        underTest.FixAllUserOwnedConsortia();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(db => db.GetLas(userCustodianCodes));
        mockDatabaseOperation.Verify(db => db.GetConsortia(expectedConsortiumCodesToAdd));
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(user, expectedConsortiaToAdd, expectedLasToRemove));

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void FixAllUserOwnedConsortia_IfOwnsAllInAConsortiaButNotEnoughInAnother_AddsOneConsortia()
    {
        // Arrange
        var consortiumAndLasInfo = GetExampleConsortiumCodesWithCustodianCodes().Take(2).ToList();
        var (consortiumCode1, custodianCodes1) = consortiumAndLasInfo[0];
        var (_, custodianCodes2) = consortiumAndLasInfo[1];
        var userCustodianCodes = custodianCodes1.Concat(custodianCodes2.Take(1)).ToList();
        var expectedConsortiumCodesToAdd = new List<string> { consortiumCode1 };

        var (user, expectedLasToRemove, expectedConsortiaToAdd) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = userCustodianCodes,
            ExpectedCustodianCodesToRemove = custodianCodes1,
            ExpectedConsortiumCodesToAdd = expectedConsortiumCodesToAdd
        });

        // Act
        underTest.FixAllUserOwnedConsortia();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(db => db.GetLas(custodianCodes1));
        mockDatabaseOperation.Verify(db => db.GetConsortia(expectedConsortiumCodesToAdd));
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(user, expectedConsortiaToAdd, expectedLasToRemove));

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void FixAllUserOwnedConsortia_IfOwnsConsortia_DoesNothing()
    {
        // Arrange
        var (consortiumCode, _) = GetExampleConsortiumCodesWithCustodianCodes().First();

        var (user, _, _) = SetupUser(new UserTestSetup
        {
            UserConsortiumCodes = new List<string> { consortiumCode }
        });

        // Act
        underTest.FixAllUserOwnedConsortia();

        // Assert
        mockDatabaseOperation.Verify(db => db.GetUsersWithLocalAuthoritiesAndConsortia());
        mockDatabaseOperation.Verify(
            db => db.AddConsortiaAndRemoveLasFromUser(
                user, It.IsAny<List<Consortium>>(), It.IsAny<List<LocalAuthority>>()),
            Times.Never);

        mockDatabaseOperation.VerifyNoOtherCalls();
    }

    [Test]
    public void FixAllUserOwnedConsortia_IfOwnsAllLaNotInConsortia_DoesNothing()
    {
        // Arrange
        var custodianCodes = new List<string> { GetExampleCustodianCodesNotInConsortium().First() };

        var (user, _, _) = SetupUser(new UserTestSetup
        {
            UserCustodianCodes = custodianCodes
        });

        // Act
        underTest.FixAllUserOwnedConsortia();

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

    private IEnumerable<(string, List<string>)> GetExampleConsortiumCodesWithCustodianCodes()
    {
        // both these Consortia have two LAs
        yield return ("C_0007", ["840", "835"]);
        yield return ("C_0025", ["1940", "1945"]);
    }

    private IEnumerable<string> GetExampleCustodianCodesNotInConsortium()
    {
        yield return "2004";
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