namespace Tests.Helpers;

using System.Collections.Generic;
using System.Linq;
using WhlgPortalWebsite.BusinessLogic.Models;

public static class ValidConsortiumGenerator
{
    public static IEnumerable<Consortium> GetConsortiaWithDifferentCodes(int count)
    {
        return ConsortiumData
            .ConsortiumNamesByConsortiumCode
            .Keys
            .Take(count)
            .Select(cc => new Consortium { ConsortiumCode = cc });
    }
}