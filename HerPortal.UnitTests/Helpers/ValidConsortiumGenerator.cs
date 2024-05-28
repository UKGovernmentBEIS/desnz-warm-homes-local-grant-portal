namespace Tests.Helpers;

using System.Collections.Generic;
using System.Linq;
using HerPortal.BusinessLogic.Models;

public class ValidConsortiumGenerator
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