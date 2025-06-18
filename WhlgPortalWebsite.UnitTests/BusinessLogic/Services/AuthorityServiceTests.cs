using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WhlgPortalWebsite.BusinessLogic;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services;

namespace Tests.BusinessLogic.Services;

public class AuthorityServiceTests
{
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private AuthorityService authorityService;

    [SetUp]
    public void Setup()
    {
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        authorityService = new AuthorityService(mockDataAccessProvider.Object);
    }

    [Test]
    public void UserManagesLocalAuthority_WhenCalledWithLAsUserOwns_ReturnsTrue()
    {
        // Arrange
        var (_, ownedLocalAuthorities) = GetExampleConsortiumCodesWithCustodianCodes().First();
        var (_, notOwnedLocalAuthorities) = GetExampleConsortiumCodesWithCustodianCodes()
            .Skip(1)
            .First();

        var user = new User
        {
            LocalAuthorities = ownedLocalAuthorities,
            Consortia = []
        };

        // Assert
        foreach (var ownedLocalAuthority in ownedLocalAuthorities)
        {
            authorityService.UserManagesLocalAuthority(user, ownedLocalAuthority).Should().BeTrue();
        }

        foreach (var notOwnedLocalAuthority in notOwnedLocalAuthorities)
        {
            authorityService.UserManagesLocalAuthority(user, notOwnedLocalAuthority).Should().BeFalse();
        }
    }

    [Test]
    public void UserManagesLocalAuthority_WhenCalledWithLAsInConsortiaUserOwns_ReturnsTrue()
    {
        // Arrange
        var (ownedConsortium, localAuthoritiesInOwnedConsortium) =
            GetExampleConsortiumCodesWithCustodianCodes().First();
        var (_, notOwnedLocalAuthorities) = GetExampleConsortiumCodesWithCustodianCodes()
            .Skip(1)
            .First();

        var user = new User
        {
            LocalAuthorities = [],
            Consortia = [ownedConsortium]
        };

        // Assert
        foreach (var localAuthorityInOwnedConsortium in localAuthoritiesInOwnedConsortium)
        {
            authorityService.UserManagesLocalAuthority(user, localAuthorityInOwnedConsortium).Should().BeTrue();
        }

        foreach (var notOwnedLocalAuthority in notOwnedLocalAuthorities)
        {
            authorityService.UserManagesLocalAuthority(user, notOwnedLocalAuthority).Should().BeFalse();
        }
    }

    [Test]
    public void UserManagesConsortium_WhenCalledWithConsortiumUserOwns_ReturnsTrue()
    {
        // Arrange
        var (ownedConsortium, _) = GetExampleConsortiumCodesWithCustodianCodes().First();
        var (notOwnedConsortium, _) = GetExampleConsortiumCodesWithCustodianCodes()
            .Skip(1)
            .First();

        var user = new User
        {
            LocalAuthorities = [],
            Consortia = [ownedConsortium]
        };

        // Assert
        authorityService.UserManagesConsortium(user, ownedConsortium).Should().BeTrue();
        authorityService.UserManagesConsortium(user, notOwnedConsortium).Should().BeFalse();
    }

    private IEnumerable<(Consortium, List<LocalAuthority>)> GetExampleConsortiumCodesWithCustodianCodes()
    {
        // both these Consortia have two LAs
        yield return (new Consortium { ConsortiumCode = "C_0006" }, [
            new LocalAuthority { CustodianCode = "840" },
            new LocalAuthority { CustodianCode = "835" }
        ]);
        yield return (new Consortium { ConsortiumCode = "C_0024" }, [
            new LocalAuthority { CustodianCode = "1940" },
            new LocalAuthority { CustodianCode = "1945" }
        ]);
    }
}