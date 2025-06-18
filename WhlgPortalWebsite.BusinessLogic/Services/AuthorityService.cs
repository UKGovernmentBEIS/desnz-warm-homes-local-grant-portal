using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services;

public interface IAuthorityService
{
    Task<IEnumerable<LocalAuthority>> SearchAllLasAsync(string searchLaName);

    Task<IEnumerable<Consortium>> SearchAllConsortiaAsync(string searchConsortiumName);
    Task<LocalAuthority> GetLocalAuthorityByCustodianCodeAsync(string custodianCode);
    Task<Consortium> GetConsortiumByConsortiumCodeAsync(string consortiumCode);
    bool UserManagesLocalAuthority(User user, LocalAuthority localAuthority);
    bool UserManagesConsortium(User user, Consortium consortium);
}

public class AuthorityService(IDataAccessProvider dataAccessProvider) : IAuthorityService
{
    public async Task<IEnumerable<LocalAuthority>> SearchAllLasAsync(string searchLaName)
    {
        return (await dataAccessProvider.GetAllLasAsync())
            .Where(la => string.IsNullOrWhiteSpace(searchLaName)
                         || LocalAuthorityData.LocalAuthorityNamesByCustodianCode[la.CustodianCode]
                             .Contains(searchLaName, StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<IEnumerable<Consortium>> SearchAllConsortiaAsync(string searchConsortiumName)
    {
        return (await dataAccessProvider.GetAllConsortiaAsync())
            .Where(consortium => string.IsNullOrWhiteSpace(searchConsortiumName)
                                 || ConsortiumData.ConsortiumNamesByConsortiumCode[consortium.ConsortiumCode]
                                     .Contains(searchConsortiumName, StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<LocalAuthority> GetLocalAuthorityByCustodianCodeAsync(string custodianCode)
    {
        return await dataAccessProvider.GetLocalAuthorityByCustodianCodeAsync(custodianCode);
    }

    public async Task<Consortium> GetConsortiumByConsortiumCodeAsync(string consortiumCode)
    {
        return await dataAccessProvider.GetConsortiumByConsortiumCodeAsync(consortiumCode);
    }

    public bool UserManagesLocalAuthority(User user, LocalAuthority localAuthority)
    {
        var managesDirectly = user
            .LocalAuthorities
            .Any(userLocalAuthority => userLocalAuthority.CustodianCode == localAuthority.CustodianCode);

        // if the user manages a consortium, they shouldn't be able to also manage an LA in that consortium
        var managesOwningConsortia = user
            .Consortia
            .Any(userConsortium => ConsortiumData
                .ConsortiumCustodianCodesIdsByConsortiumCode[userConsortium.ConsortiumCode]
                .Any(consortiumCustodianCode => consortiumCustodianCode == localAuthority.CustodianCode));

        return managesDirectly || managesOwningConsortia;
    }

    public bool UserManagesConsortium(User user, Consortium consortium)
    {
        return user.Consortia.Any(userConsortium => userConsortium.ConsortiumCode == consortium.ConsortiumCode);
    }
}