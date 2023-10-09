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
}