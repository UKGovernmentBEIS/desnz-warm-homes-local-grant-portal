using System;
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
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052", "2525" };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);

        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Confirm(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public void CreatesNewUser_IfUserNotFoundByDbOperation()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052"};
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);
        var las = new List<LocalAuthority>
        {
            new()
            {
                Id = 1,
                CustodianCode = "9052"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetLas(custodianCodes)).Returns(las);
        
        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);
        
        // Assert
        mockDatabaseOperation.Verify(mock => mock.CreateUserOrLogError(userEmailAddress, las), Times.Once());
    }

    [Test]
    public void AddsLasToExistingUser_IfUserFoundByDbOperation()
    {
        // Arrange
        var currentLa = new List<LocalAuthority>
        {
            new LocalAuthority()
            {
                Id = 2,
                CustodianCode = "2525"
            }
        };
        const string userEmailAddress = "existinguser@email.com";

        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").WithLocalAuthorities(currentLa).Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        
        var custodianCodes = new[] { "9052"};
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);
        
        var lasToAdd = new List<LocalAuthority>
        {
            new()
            {
                Id = 1,
                CustodianCode = "9052"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetLas(custodianCodes)).Returns(lasToAdd);
        
        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);
        
        // Assert
        mockDatabaseOperation.Verify(mock => mock.AddLasToUser(users[0], lasToAdd));
    }

    [Test]
    public void RemovesLasFromExistingUser_IfUserFoundByDbOperation()
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
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);

        // Act
        underTest.RemoveLas(user, custodianCodes);

        // Assert
        mockDatabaseOperation.Verify(mock => mock.RemoveLasFromUser(user, new List<LocalAuthority> { laToRemove }), Times.Once());
    }

    [Test]
    public void RemoveUser_IfUserFound_WhenThereIsDeletionConfirmation()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        mockOutputProvider.Setup(mock => mock.Confirm(It.IsAny<string>())).Returns(true);
        
        // Act
        underTest.TryRemoveUser(users[0]);
        
        // Assert
        mockDatabaseOperation.Verify(mock => mock.RemoveUserOrLogError(users[0]), Times.Once());
    }

    [Test]
    public void DisplaysErrorMessage_WhenNoLasSpecified_IfUserExists()
    {
        // Arrange
        var userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        SetupConfirmCustodianCodes(Array.Empty<string>(), userEmailAddress, true);

        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, Array.Empty<string>());
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output(It.IsAny<string>()));
    }

    [Test]
    public void DisplaysErrorMessage_IfCustodianCodeToRemove_DoesNotMatchAnyOfExistingUsersLas()
    {
        // Arrange
        var laToRemove = new LocalAuthority
        {
            CustodianCode = "9052",
            Id = 123
        };

        var usersCurrentLa = new LocalAuthority
        {
            CustodianCode = "2525",
            Id = 7
        };
        
        var userEmailAddress = "existinguser@email.com";
        var user = new UserBuilder(userEmailAddress).WithLocalAuthorities(new List<LocalAuthority> { usersCurrentLa }).Build();
        var users = new List<User> { user };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);

        var custodianCodes = new[] { laToRemove.CustodianCode };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);
        
        // Act
        underTest.RemoveLas(user, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output(It.IsAny<string>()));
    }

    [Test]
    public void DisplaysInnerExceptionMessage_IfCustodianCode_IsNotFoundInDict()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var listWithCustodianCodeNotInDict = new [] { "1111" };

        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, listWithCustodianCodeNotInDict);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output(It.IsAny<string>()));
    }

    [Test]
    public void DisplaysErrorMessage_WhenRemovingLas_IfUserNotInDatabase()
    {
        // Arrange
        const string userEmailAddress = "usernotindb@email.com";
        var users = new List<User>
        {
            new UserBuilder("userindb@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052"};
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, false);
        var lasToAdd = new List<LocalAuthority>
        {
            new()
            {
                Id = 1,
                CustodianCode = "9052"
            }
        };
        
        // Act
        underTest.RemoveLas(null, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User not found"));
    }

    [Test]
    public void DoesNotCallDbOperation_IfConfirmKeyNotPressed()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052"};
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, false);
        var lasToAdd = new List<LocalAuthority>
        {
            new()
            {
                Id = 1,
                CustodianCode = "9052"
            }
        };
        
        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("Process cancelled, no changes were made to the database"));
        mockDatabaseOperation.Verify(mock => mock.CreateUserOrLogError(userEmailAddress, lasToAdd), Times.Never());
    }
    
    [Test]
    public void AsksForConfirmation()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052"};
        
        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Confirm("Please confirm (y/n)"), Times.Once);
    }
    
    [Test]
    public void DisplaysCorrectUserStatus_WhenUserActive()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052"};
        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User found in database. LAs will be added to their account"), Times.Once());
    }
    
    [Test]
    public void DisplaysCorrectUserStatus_WhenUserInactive()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthorities()).Returns(users);
        var custodianCodes = new[] { "9052"};
        // Act
        underTest.CreateOrUpdateUser(userEmailAddress, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User not found in database. A new user will be created"), Times.Once());
    }
    
    private void SetupConfirmCustodianCodes(IEnumerable<string> custodianCodes, string userEmailAddress, bool confirmation)
    {
        mockOutputProvider
            .Setup(op =>
                op.Output(
                    $"You are changing permissions for user {userEmailAddress} for the following local authorities: "));
        foreach (var code in custodianCodes)
        {
            mockOutputProvider.Setup(op => op.Output("Code: Local Authority"));
        }

        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(confirmation);
    }
}