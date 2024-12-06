using System.Collections.Generic;
using System.Linq;
using WhlgPortalWebsite.BusinessLogic.Models;

namespace Tests.Helpers;

public static class ValidLocalAuthorityGenerator
{
    public static IEnumerable<LocalAuthority> GetLocalAuthoritiesWithDifferentCodes(int count)
    {
        return LocalAuthorityData
            .LocalAuthorityNamesByCustodianCode
            .Keys
            .Take(count)
            .Select(cc => new LocalAuthority { CustodianCode = cc });
    }
}
