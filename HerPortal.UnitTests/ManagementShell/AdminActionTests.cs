using System.Collections.Generic;
using HerPortal.BusinessLogic.Models;
using Moq;
using NUnit.Framework;
using Tests.Builders;
using HerPortal.ManagementShell;

namespace Tests.ManagementShell;

public class AdminActionTests
{
    private Mock<IOutputProvider> mockOutputProvider;
    private Mock<IDatabaseOperation> mockDatabaseOperation;
    private AdminAction underTest;

    [SetUp]
    public void Setup()
    {
        mockOutputProvider = new Mock<IOutputProvider>();
        mockDatabaseOperation = new Mock<IDatabaseOperation>();
        underTest = new AdminAction(mockDatabaseOperation.Object, mockOutputProvider.Object);
    }

    [Test]
    public void FindsExistingUserCaseInsensitively_IfInDatabase()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build(),
            new UserBuilder("existinguser2@email.com").Build()
        };
        const string userEmailAddress = "ExistingUser@email.com";
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);

        // Act
        var returnedUser = underTest.GetUser(userEmailAddress);

        // Assert
        Assert.AreEqual(users[0].EmailAddress, returnedUser!.EmailAddress);
    }

    [Test]
    public void ConfirmsCustodianCodesWhenUpdating()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build(),
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052", "2525" };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress);

        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Confirm(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public void CreatesANewUser_WhenEmailNotInDatabase()
    {

    }

    [Test]
    public void RollsBackTransaction_IfUserCreationFails()
    {

    }

    [Test]
    public void RemovesLasFromUser()
    {
        // Arrange
        var laToRemove = new LocalAuthority
        {
            CustodianCode = "9052",
            Id = 123
        };

        var laToKeep = new LocalAuthority
        {
            CustodianCode = "456",
            Id = 456
        };

        var userEmailAddress = "existinguser@email.com";
        var user = new UserBuilder(userEmailAddress).WithLocalAuthorities(new List<LocalAuthority> { laToRemove, laToKeep }).Build();
        var users = new List<User> { user };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);

        var custodianCodes = new[] { laToRemove.CustodianCode };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress);

        // Act
        underTest.RemoveLas(user, custodianCodes);

        // Assert
        mockDatabaseOperation.Verify(mock => mock.RemoveLasFromUser(user, new List<LocalAuthority> { laToRemove }), Times.Once());
    }

    private void SetupConfirmCustodianCodes(IEnumerable<string> custodianCodes, string userEmailAddress)
    {
        // Arrange
        mockOutputProvider
            .Setup(op =>
                op.Output(
                    $"You are changing permissions for user {userEmailAddress} for the following local authorities: "));
        foreach (var code in custodianCodes)
        {
            mockOutputProvider.Setup(op => op.Output("Code: Local Authority"));
        }

        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);
    }
}