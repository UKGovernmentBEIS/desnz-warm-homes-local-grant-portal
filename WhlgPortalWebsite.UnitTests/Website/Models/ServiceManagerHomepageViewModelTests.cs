using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tests.Builders;
using Tests.Helpers;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Models;

[TestFixture]
public class ServiceManagerHomepageViewModelTests
{
    private const string ExampleEmail1 = "email@example.com";
    private const string ExampleEmail2 = "email2@example.com";

    [Test]
    public void HomepageViewModel_ContainsUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder(ExampleEmail1).Build(),
            new UserBuilder(ExampleEmail2).Build()
        };

        // Act
        var viewModel = new ServiceManagerHomepageViewModel(users);

        // Assert
        var userListings = viewModel.UserList.ToList();
        userListings.Count.Should().Be(2);
    }

    [Test]
    public void HomepageViewModel_ContainsUserIds()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder(ExampleEmail1)
                .WithId(1)
                .Build(),
            new UserBuilder(ExampleEmail2)
                .WithId(2)
                .Build()
        };

        // Act
        var viewModel = new ServiceManagerHomepageViewModel(users);

        // Assert
        var userListings = viewModel.UserList.ToList();
        userListings[0].Id.Should().Be(1);
        userListings[1].Id.Should().Be(2);
    }

    [Test]
    public void HomepageViewModel_ContainsUserEmails()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder(ExampleEmail1).Build(),
            new UserBuilder(ExampleEmail2).Build()
        };

        // Act
        var viewModel = new ServiceManagerHomepageViewModel(users);

        // Assert
        var userListings = viewModel.UserList.ToList();
        userListings[0].EmailAddress.Should().Be(ExampleEmail1);
        userListings[1].EmailAddress.Should().Be(ExampleEmail2);
    }

    [Test]
    public void HomepageViewModel_ContainsOwnedLas()
    {
        // Arrange
        var las = ValidLocalAuthorityGenerator.GetLocalAuthoritiesWithDifferentCodes(3).ToList();

        var users = new List<User>
        {
            new UserBuilder(ExampleEmail1)
                .WithLocalAuthorities(las)
                .Build()
        };

        var namesCombined = string.Join(", ",
            las.Select(la => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[la.CustodianCode]));

        // Act
        var viewModel = new ServiceManagerHomepageViewModel(users);

        // Assert
        var userListings = viewModel.UserList.ToList();
        userListings[0].Manages.Should().Be(namesCombined);
    }

    [Test]
    public void HomepageViewModel_ContainsOwnedConsortia()
    {
        // Arrange
        var consortia = ValidConsortiumGenerator.GetConsortiaWithDifferentCodes(3).ToList();

        var users = new List<User>
        {
            new UserBuilder(ExampleEmail1)
                .WithConsortia(consortia)
                .Build()
        };

        var namesCombined = string.Join(", ",
            consortia.Select(la =>
                $"{ConsortiumData.ConsortiumNamesByConsortiumCode[la.ConsortiumCode]} (Consortium)"));

        // Act
        var viewModel = new ServiceManagerHomepageViewModel(users);

        // Assert
        var userListings = viewModel.UserList.ToList();
        userListings[0].Manages.Should().Be(namesCombined);
    }

    [Test]
    public void HomepageViewModel_WhenUserManagesBothLaAndConsortia_ListsConsortiaFirst()
    {
        // Arrange
        var las = ValidLocalAuthorityGenerator.GetLocalAuthoritiesWithDifferentCodes(3).ToList();
        var consortia = ValidConsortiumGenerator.GetConsortiaWithDifferentCodes(3).ToList();

        var users = new List<User>
        {
            new UserBuilder(ExampleEmail1)
                .WithLocalAuthorities(las)
                .WithConsortia(consortia)
                .Build()
        };

        var laNamesCombined = string.Join(", ",
            las.Select(la => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[la.CustodianCode]));
        var consortiumNamesCombined = string.Join(", ",
            consortia.Select(la => $"{ConsortiumData.ConsortiumNamesByConsortiumCode[la.ConsortiumCode]} (Consortium)"));
        var namesCombined = $"{consortiumNamesCombined}, {laNamesCombined}";

        // Act
        var viewModel = new ServiceManagerHomepageViewModel(users);

        // Assert
        var userListings = viewModel.UserList.ToList();
        userListings[0].Manages.Should().Be(namesCombined);
    }
}