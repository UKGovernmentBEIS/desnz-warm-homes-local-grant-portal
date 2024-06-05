using System;
using System.Collections.Generic;
using HerPortal.BusinessLogic.Models;
using HerPortal.ManagementShell;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.ManagementShell;

public class CommandHandlerTests
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
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

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
        var custodianCodes = new[] { "9052" };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);
        var las = new List<LocalAuthority>
        {
            new()
            {
                Id = 1,
                CustodianCode = "9052"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetLas(custodianCodes)).Returns(las);
        mockDatabaseOperation.Setup(mock => mock.GetConsortia(It.IsAny<string[]>())).Returns(new List<Consortium>());

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
            new()
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
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

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
    public void CreateOrUpdateUserWithLas_AddsLasToExistingUser_IgnoresLasInOwnedConsortia()
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

        // 2372 is in consortium C_0002
        var custodianCodes = new[] { "9052", "2372" };
        var filteredCustodianCodes = new[] { "9052" };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        var lasToAdd = new List<LocalAuthority>
        {
            new()
            {
                Id = 1,
                CustodianCode = "9052"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetLas(filteredCustodianCodes)).Returns(lasToAdd);

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
        var user = new UserBuilder(userEmailAddress)
            .WithLocalAuthorities(new List<LocalAuthority> { laToRemove, laToKeep }).Build();
        var users = new List<User> { user };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);

        var custodianCodes = new[] { laToRemove.CustodianCode };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        // Act
        underTest.TryRemoveLas(user, custodianCodes);

        // Assert
        mockDatabaseOperation.Verify(mock => mock.RemoveLasFromUser(user, new List<LocalAuthority> { laToRemove }),
            Times.Once());
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
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, Array.Empty<string>());

        // Assert
        mockOutputProvider.Verify(mock => mock.Output("Please specify custodian codes to add to user"));
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
        var user = new UserBuilder(userEmailAddress).WithLocalAuthorities(new List<LocalAuthority> { usersCurrentLa })
            .Build();
        var users = new List<User> { user };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);

        var custodianCodes = new[] { laToRemove.CustodianCode };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        // Act
        underTest.TryRemoveLas(user, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Output("Invalid Codes: 9052"));
        mockOutputProvider.Verify(mock => mock.Output("Custodian Codes are not associated with this user."));
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
        var listWithCustodianCodeNotInDict = new[] { "1111" };

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
        var users = new List<User>
        {
            new UserBuilder("userindb@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var custodianCodes = new[] { "9052" };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);
        var lasToAdd = new List<LocalAuthority>
        {
            new()
            {
                Id = 1,
                CustodianCode = "9052"
            }
        };

        // Act
        underTest.TryRemoveLas(null, custodianCodes);

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
        var custodianCodes = new[] { "9052" };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(false);
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
        var custodianCodes = new[] { "9052" };

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
        var custodianCodes = new[] { "9052" };
        // Act
        underTest.CreateOrUpdateUserWithLas(userEmailAddress, custodianCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User found in database. LAs will be added to their account"),
            Times.Once());
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
        mockOutputProvider.Verify(mock => mock.Output("User not found in database. A new user will be created"),
            Times.Once());
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
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);
        mockDatabaseOperation.Setup(mock => mock.GetLas(It.IsAny<List<string>>())).Returns(new List<LocalAuthority>());

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
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);
        var consortia = new List<Consortium>
        {
            new()
            {
                Id = 1,
                ConsortiumCode = "C_0002"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetConsortia(consortiumCodes)).Returns(consortia);
        mockDatabaseOperation.Setup(mock => mock.GetLas(It.IsAny<string[]>())).Returns(new List<LocalAuthority>());

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);

        // Assert
        mockDatabaseOperation.Verify(
            mock => mock.CreateUserOrLogError(userEmailAddress, It.IsAny<List<LocalAuthority>>(), consortia),
            Times.Once());
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

        var consortiumCodes = new[] { "C_0003" };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        var consortiaToAdd = new List<Consortium>
        {
            new()
            {
                Id = 1,
                ConsortiumCode = "C_0003"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetLas(It.IsAny<List<string>>())).Returns(new List<LocalAuthority>());
        mockDatabaseOperation.Setup(mock => mock.GetConsortia(consortiumCodes)).Returns(consortiaToAdd);

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);

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
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, Array.Empty<string>());

        // Assert
        mockOutputProvider.Verify(mock => mock.Output("Please specify consortium codes to add to user"));
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
        var listWithCustodianCodeNotInDict = new[] { "C_1111" };

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
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(false);
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
            mock => mock.CreateUserOrLogError(userEmailAddress, It.IsAny<List<LocalAuthority>>(), consortiaToAdd),
            Times.Never());
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
        mockOutputProvider.Verify(mock => mock.Output("User found in database. LAs will be added to their account"),
            Times.Once());
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
        mockOutputProvider.Verify(mock => mock.Output("User not found in database. A new user will be created"),
            Times.Once());
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
        mockOutputProvider.Verify(mock => mock.Output("2372: Blackburn With Darwen (Blackpool)"), Times.Once());
    }

    [Test]
    public void CreateOrUpdateUserWithConsortia_WhenUserHasLasInConsortia_RemovesThem()
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
        var custodianCodesToRemove = new[] { "2372" };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        var consortiaToAdd = new List<Consortium>
        {
            new()
            {
                Id = 1,
                ConsortiumCode = "C_0002"
            }
        };
        mockDatabaseOperation.Setup(mock => mock.GetConsortia(consortiumCodes)).Returns(consortiaToAdd);
        mockDatabaseOperation.Setup(mock => mock.GetLas(custodianCodesToRemove)).Returns(currentLa);

        // Act
        underTest.CreateOrUpdateUserWithConsortia(userEmailAddress, consortiumCodes);

        // Assert
        mockDatabaseOperation.Verify(mock =>
            mock.AddConsortiaAndRemoveLasFromUser(users[0], consortiaToAdd, currentLa));
    }

    [Test]
    public void RemoveConsortia_RemovesLasFromExistingUser_IfUserFoundByDbOperation()
    {
        // Arrange
        var consortiumToRemove = new Consortium
        {
            ConsortiumCode = "C_0002",
            Id = 123
        };

        var consortiumToKeep = new Consortium
        {
            ConsortiumCode = "C_0003",
            Id = 456
        };

        var userEmailAddress = "existinguser@email.com";
        var user = new UserBuilder(userEmailAddress)
            .WithConsortia(new List<Consortium> { consortiumToRemove, consortiumToKeep }).Build();
        var users = new List<User> { user };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);

        var custodianCodes = new[] { consortiumToRemove.ConsortiumCode };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        // Act
        underTest.TryRemoveConsortia(user, custodianCodes);

        // Assert
        mockDatabaseOperation.Verify(
            mock => mock.RemoveConsortiaFromUser(user, new List<Consortium> { consortiumToRemove }),
            Times.Once());
    }

    [Test]
    public void RemoveConsortia_DisplaysErrorMessage_IfCustodianCodeToRemove_DoesNotMatchAnyOfExistingUsersLas()
    {
        // Arrange
        var consortiumToRemove = new Consortium
        {
            ConsortiumCode = "C_0002",
            Id = 123
        };

        var usersCurrentConsortium = new Consortium
        {
            ConsortiumCode = "C_0003",
            Id = 7
        };

        var userEmailAddress = "existinguser@email.com";
        var user = new UserBuilder(userEmailAddress).WithConsortia(new List<Consortium> { usersCurrentConsortium })
            .Build();
        var users = new List<User> { user };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);

        var consortiumCodes = new[] { consortiumToRemove.ConsortiumCode };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        // Act
        underTest.TryRemoveConsortia(user, consortiumCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Output("Invalid Codes: C_0002"));
        mockOutputProvider.Verify(mock => mock.Output("Consortium Codes are not associated with this user."));
    }

    [Test]
    public void RemoveConsortia_DisplaysErrorMessage_WhenRemovingLas_IfUserNotInDatabase()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder("userindb@email.com").Build()
        };
        mockDatabaseOperation.Setup(db => db.GetUsersWithLocalAuthoritiesAndConsortia()).Returns(users);
        var consortiumCodes = new[] { "C_0002" };
        mockOutputProvider.Setup(op => op.Confirm("Please confirm (y/n)")).Returns(true);

        // Act
        underTest.TryRemoveConsortia(null, consortiumCodes);

        // Assert
        mockOutputProvider.Verify(mock => mock.Output("User not found"));
    }
}