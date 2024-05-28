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
    public void GetUser_FindsExistingUserCaseInsensitively_IfInDatabase()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build(),
            new UserBuilder("existinguser2@email.com").Build()
        };
        const string userEmailAddress = "ExistingUser@email.com";
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);

        // Act
        var returnedUser = underTest.GetUser(userEmailAddress);

        // Assert
        Assert.AreEqual(users[0].EmailAddress, returnedUser!.EmailAddress);
    }

    [Test]
    public void CreateOrUpdateUserWithLas_ConfirmsCustodianCodesWhenUpdating()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var custodianCodes = new[] { "9052", "2525" };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);

        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Confirm(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public void CreateOrUpdateUserWithLas_CreatesNewUser_IfUserNotFoundByDbOperation()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
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
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);
        
        // Assert
        mockDatabaseOperation.Verify(
            mock => mock.CreateUserOrLogError(userEmailAddress, las, It.IsAny<List<Consortium>>()), Times.Once());
    }

    [Test]
    public void CreateOrUpdateUserWithLas_AddsLasToExistingUser_IfUserFoundByDbOperation()
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
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        
        var custodianCodes = new[] { "9052" };
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
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);
        
        // Assert
        mockDatabaseOperation.Verify(mock => mock.AddLasToUser(users[0], lasToAdd));
    }

    [Test]
    public void RemoveLas_RemovesLasFromExistingUser_IfUserFoundByDbOperation()
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
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);

        var custodianCodes = new[] { laToRemove.CustodianCode };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);

        // Act
        underTest.RemoveLas(user, custodianCodes);

        // Assert
        mockDatabaseOperation.Verify(mock => mock.RemoveLasFromUser(user, new List<LocalAuthority> { laToRemove }), Times.Once());
    }

    [Test]
    public void TryRemoveUser_IfUserFound_WhenThereIsDeletionConfirmation()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        mockOutputProvider.Setup(mock => mock.Confirm(It.IsAny<string>())).Returns(true);
        
        // Act
        underTest.TryRemoveUser(users[0]);
        
        // Assert
        mockDatabaseOperation.Verify(mock => mock.RemoveUserOrLogError(users[0]), Times.Once());
    }

    [Test]
    public void CreateOrUpdateUserWithLas_DisplaysErrorMessage_WhenNoLasSpecified_IfUserExists()
    {
        // Arrange
        var userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        SetupConfirmCustodianCodes(Array.Empty<string>(), userEmailAddress, true);

        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, Array.Empty<string>());
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output(It.IsAny<string>()));
    }

    [Test]
    public void RemoveLas_DisplaysErrorMessage_IfCustodianCodeToRemove_DoesNotMatchAnyOfExistingUsersLas()
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
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);

        var custodianCodes = new[] { laToRemove.CustodianCode };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);
        
        // Act
        underTest.RemoveLas(user, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output(It.IsAny<string>()));
    }

    [Test]
    public void CreateOrUpdateUserWithLas_DisplaysInnerExceptionMessage_IfCustodianCode_IsNotFoundInDict()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var listWithCustodianCodeNotInDict = new [] { "1111" };

        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, listWithCustodianCodeNotInDict);
        
        // Assert
        mockOutputProvider.Verify(mock =>
            mock.Output("The given key '1111' was not present in the dictionary. Process terminated"));
    }

    [Test]
    public void RemoveLas_DisplaysErrorMessage_WhenRemovingLas_IfUserNotInDatabase()
    {
        // Arrange
        const string userEmailAddress = "usernotindb@email.com";
        var users = new List<User>
        {
            new UserBuilder("userindb@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
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
    public void CreateOrUpdateUserWithLas_DoesNotCallDbOperation_IfConfirmKeyNotPressed()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
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
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("Process cancelled, no changes were made to the database"));
        mockDatabaseOperation.Verify(
            mock => mock.CreateUserOrLogError(userEmailAddress, lasToAdd, It.IsAny<List<Consortium>>()), Times.Never());
    }
    
    [Test]
    public void CreateOrUpdateUserWithLas_AsksForConfirmation()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var custodianCodes = new[] { "9052"};
        
        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Confirm("Please confirm (y/n)"), Times.Once);
    }
    
    [Test]
    public void CreateOrUpdateUserWithLas_DisplaysCorrectUserStatus_WhenUserActive()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var custodianCodes = new[] { "9052"};
        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User found in database. LAs will be added to their account"), Times.Once());
    }
    
    [Test]
    public void CreateOrUpdateUserWithLas_DisplaysCorrectUserStatus_WhenUserInactive()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var custodianCodes = new[] { "9052" };
        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User not found in database. A new user will be created"), Times.Once());
    }
    
    [Test]
    public void CreateOrUpdateUserWithLas_WhenGivenLocalAuthorities_PrintsThem()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var custodianCodes = new[] { "9052", "3805" };
        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("9052: Aberdeenshire"), Times.Once());
        mockOutputProvider.Verify(mock => mock.Output("3805: Adur"), Times.Once());
    }
    
    private void SetupConfirmCustodianCodes(IEnumerable<string> custodianCodes, string userEmailAddress, bool confirmation)
    {
        mockOutputProvider
            .Setup(op =>
                op.Output(
                    $"You are changing permissions for user {userEmailAddress} for the following Local Authorities:"));
        mockOutputProvider
            .Setup(op => op.Output("Add the following Local Authorities:"));
        foreach (var code in custodianCodes)
        {
            mockOutputProvider.Setup(op => op.Output("Code: Local Authority"));
        }
        mockOutputProvider
            .Setup(op => 
                op.Output("Ignore the following Local Authorities already in owned Consortia:"));
        mockOutputProvider.Setup(op => op.Output("(None)"));

        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(confirmation);
    }
    
    [Test]
    public void CreateOrUpdateUserWithConsortia_ConfirmsCustodianCodesWhenUpdating()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var custodianCodes = new[] { "C_0002", "C_0003" };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Confirm(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public void CreateOrUpdateUserWithConsortia_CreatesNewUser_IfUserNotFoundByDbOperation()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002" };
        SetupConfirmCustodianCodes(consortiumCodes, userEmailAddress, true);
        var consortia = new List<Consortium>
        {
            new()
            {
                Id = 1,
                ConsortiumCode = "C_0002"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetConsortia(consortiumCodes)).Returns(consortia);
        
        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);
        
        // Assert
        mockDatabaseOperation.Verify(
            mock => mock.CreateUserOrLogError(userEmailAddress, It.IsAny<List<LocalAuthority>>(), consortia), Times.Once());
    }

    [Test]
    public void CreateOrUpdateUserWithConsortia_AddsLasToExistingUser_IfUserFoundByDbOperation()
    {
        // Arrange
        var currentConsortia = new List<Consortium>
        {
            new()
            {
                Id = 2,
                ConsortiumCode = "C_0002"
            }
        };
        const string userEmailAddress = "existinguser@email.com";

        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").WithConsortia(currentConsortia).Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        
        var custodianCodes = new[] { "C_0002" };
        SetupConfirmCustodianCodes(custodianCodes, userEmailAddress, true);
        
        var consortiaToAdd = new List<Consortium>
        {
            new()
            {
                Id = 1,
                ConsortiumCode = "C_0003"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetConsortia(custodianCodes)).Returns(consortiaToAdd);
        
        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, custodianCodes);
        
        // Assert
        mockDatabaseOperation.Verify(mock => mock.AddConsortiaToUser(users[0], consortiaToAdd));
    }


    [Test]
    public void CreateOrUpdateUserWithConsortia_DisplaysErrorMessage_WhenNoLasSpecified_IfUserExists()
    {
        // Arrange
        var userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        SetupConfirmCustodianCodes(Array.Empty<string>(), userEmailAddress, true);

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, Array.Empty<string>());
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output(It.IsAny<string>()));
    }


    [Test]
    public void CreateOrUpdateUserWithConsortia_DisplaysInnerExceptionMessage_IfCustodianCode_IsNotFoundInDict()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var listWithCustodianCodeNotInDict = new [] { "C_1111" };

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, listWithCustodianCodeNotInDict);
        
        // Assert
        mockOutputProvider.Verify(mock =>
            mock.Output("The given key 'C_1111' was not present in the dictionary. Process terminated"));
    }


    [Test]
    public void CreateOrUpdateUserWithConsortia_DoesNotCallDbOperation_IfConfirmKeyNotPressed()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002" };
        SetupConfirmConsortiumCodes(consortiumCodes, userEmailAddress, false);
        var consortiaToAdd = new List<Consortium>
        {
            new()
            {
                Id = 1,
                ConsortiumCode = "C_0002"
            }
        };

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Output("Process cancelled, no changes were made to the database"));
        mockDatabaseOperation.Verify(
            mock => mock.CreateUserOrLogError(userEmailAddress, It.IsAny<List<LocalAuthority>>(), consortiaToAdd), Times.Never());

    }

    [Test]
    public void CreateOrUpdateUserWithConsortia_AsksForConfirmation()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002" };
        
        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Confirm("Please confirm (y/n)"), Times.Once);
    }
    
    [Test]
    public void CreateOrUpdateUserWithConsortia_DisplaysCorrectUserStatus_WhenUserActive()
    {
        // Arrange
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
   
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002" };
        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User found in database. LAs will be added to their account"), Times.Once());
    }
    
    [Test]
    public void CreateOrUpdateUserWithConsortia_DisplaysCorrectUserStatus_WhenUserInactive()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002" };
        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User not found in database. A new user will be created"), Times.Once());
    }
    
    [Test]
    public void CreateOrUpdateUserWithConsortia_WhenGivenConsortia_PrintsThem()
    {
        // Arrange
        const string userEmailAddress = "newuser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002", "C_0003" };
        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("C_0002: Blackpool"), Times.Once());
        mockOutputProvider.Verify(mock => mock.Output("C_0003: Bristol"), Times.Once());
    }
    
    [Test]
    public void CreateOrUpdateUserWithConsortia_WhenUserOwnsLocalAuthorityInConsortium_PrintsThem()
    {
        // Arrange
        var currentLa = new List<LocalAuthority>
        {
            new()
            {
                Id = 2,
                CustodianCode = "2372"
            }
        };
        
        const string userEmailAddress = "existinguser@email.com";
        var users = new List<User>
        {
            new UserBuilder("existinguser@email.com").WithLocalAuthorities(currentLa).Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002" };
        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);
        
        // Assert
        mockOutputProvider.Verify(mock => mock.Output("2372: Blackburn With Darwen"), Times.Once());
    }
    
    private void SetupConfirmConsortiumCodes(IEnumerable<string> consortiumCodes, string userEmailAddress, bool confirmation)
    {
        mockOutputProvider
            .Setup(op =>
                op.Output(
                    $"You are changing permissions for user {userEmailAddress} for the following consortiums: "));
        foreach (var code in consortiumCodes)
        {
            mockOutputProvider.Setup(op => op.Output("Code: Consortium"));
        }

        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(confirmation);
    }
}