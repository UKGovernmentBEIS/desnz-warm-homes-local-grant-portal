using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.Models;

namespace Tests.Website.Models;

public class AssignCodesToDeliveryPartnerViewModelTests
{
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

    [Test]
    public void GetLocalAuthoritiesToAssign_UserAlreadyAssignedLA_ShouldReturnCorrectAssignments()
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

        var viewModel = new AssignCodesToDeliveryPartnerViewModel
        {
            User = user,
            LocalAuthorities = ownedLocalAuthorities.Concat(notOwnedLocalAuthorities).ToList()
        };

        // Act
        var result = viewModel.GetLocalAuthoritiesToAssign().ToList();

        // Assert
        result.Should().HaveCount(ownedLocalAuthorities.Count + notOwnedLocalAuthorities.Count);
        result.Where(x => ownedLocalAuthorities.Any(la => la.CustodianCode == x.Code))
            .Should().OnlyContain(x => x.AlreadyAssigned);
        result.Where(x => notOwnedLocalAuthorities.Any(la => la.CustodianCode == x.Code))
            .Should().OnlyContain(x => !x.AlreadyAssigned);
    }


    [Test]
    public void GetConsortiaToAssign_UserAlreadyAssignedConsortia_ShouldReturnCorrectAssignments()
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

        var viewModel = new AssignCodesToDeliveryPartnerViewModel
        {
            User = user,
            Consortia = [ownedConsortium, notOwnedConsortium]
        };

        // Act
        var result = viewModel.GetConsortiaToAssign().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(x => x.Code == ownedConsortium.ConsortiumCode && x.AlreadyAssigned);
        result.Should().ContainSingle(x => x.Code == notOwnedConsortium.ConsortiumCode && !x.AlreadyAssigned);
    }

    [Test]
    public void GetLocalAuthoritiesToAssign_UserAlreadyAssignedConsortia_ShouldReturnCorrectAssignments()
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

        var viewModel = new AssignCodesToDeliveryPartnerViewModel
        {
            User = user,
            LocalAuthorities = localAuthoritiesInOwnedConsortium.Concat(notOwnedLocalAuthorities).ToList()
        };

        // Act
        var result = viewModel.GetLocalAuthoritiesToAssign().ToList();

        // Assert
        result.Should().HaveCount(localAuthoritiesInOwnedConsortium.Count + notOwnedLocalAuthorities.Count);
        result.Where(x => localAuthoritiesInOwnedConsortium.Any(la => la.CustodianCode == x.Code))
            .Should().OnlyContain(x => x.AlreadyAssigned);
        result.Where(x => notOwnedLocalAuthorities.Any(la => la.CustodianCode == x.Code))
            .Should().OnlyContain(x => !x.AlreadyAssigned);
    }
}