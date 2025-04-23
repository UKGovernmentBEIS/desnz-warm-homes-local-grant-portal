using WhlgPortalWebsite.BusinessLogic.Models.Enums;

namespace WhlgPortalWebsite.BusinessLogic.Models;

public class User : IEntityWithRowVersioning
{
    public int Id { get; set; }
    public string EmailAddress { get; set; }
    public bool HasLoggedIn { get; set; }
    public uint Version { get; set; }

    public List<LocalAuthority> LocalAuthorities { get; set; }
    public List<Consortium> Consortia { get; set; }

    public UserRole Role { get; set; }

    public IEnumerable<string> GetAdministeredCustodianCodes()
    {
        var consortiumCodes = Consortia.Select(consortium => consortium.ConsortiumCode);
        var custodianCodes = LocalAuthorities.Select(la => la.CustodianCode);

        return consortiumCodes.SelectMany(consortiumCode =>
                ConsortiumData.ConsortiumCustodianCodesIdsByConsortiumCode[consortiumCode])
            .Union(custodianCodes);
    }

    public IEnumerable<string> GetAdministeredConsortiumCodes()
    {
        return Consortia.Select(c => c.ConsortiumCode);
    }
}