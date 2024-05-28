namespace HerPortal.BusinessLogic.Models;

public class User
{
    public int Id { get; set; }
    public string EmailAddress { get; set; }
    public bool HasLoggedIn { get; set; }

    public List<LocalAuthority> LocalAuthorities { get; set; }
    public List<Consortium> Consortia { get; set; }

    public List<string> GetAdministratedCustodianCodes()
    {
        var consortiumCodes = Consortia.Select(consortium => consortium.ConsortiumCode).ToList();
        var custodianCodes = LocalAuthorities.Select(la => la.CustodianCode).ToList();
        return LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode
            .Where(codes => consortiumCodes.Contains(codes.Value))
            .Select(codes => codes.Key)
            .Union(custodianCodes)
            .ToList();
    }
}