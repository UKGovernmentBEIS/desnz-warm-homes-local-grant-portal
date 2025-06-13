using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services;

public interface IAuthorityService
{
    Task<IEnumerable<LocalAuthority>> SearchAllLasAsync(string searchLaName);

    Task<IEnumerable<Consortium>> SearchAllConsortiaAsync(string searchConsortiumName);
}

public class AuthorityService(IDataAccessProvider dataAccessProvider) : IAuthorityService
{
    public async Task<IEnumerable<LocalAuthority>> SearchAllLasAsync(string searchLaName)
    {
        return string.IsNullOrWhiteSpace(searchLaName) switch
        {
            true => await dataAccessProvider.GetAllLasAsync(),
            false => (await dataAccessProvider.GetAllLasAsync())
                .Where(la =>
                    LocalAuthorityData.LocalAuthorityNamesByCustodianCode[la.CustodianCode]
                        .Contains(searchLaName, StringComparison.CurrentCultureIgnoreCase))
        };
    }

    public async Task<IEnumerable<Consortium>> SearchAllConsortiaAsync(string searchConsortiumName)
    {
        return string.IsNullOrWhiteSpace(searchConsortiumName) switch
        {
            true => await dataAccessProvider.GetAllConsortiaAsync(),
            false => (await dataAccessProvider.GetAllConsortiaAsync())
                .Where(la =>
                    ConsortiumData.ConsortiumNamesByConsortiumCode[la.ConsortiumCode]
                        .Contains(searchConsortiumName, StringComparison.CurrentCultureIgnoreCase))
        };
    }

    public async Task<IEnumerable<Consortium>> SearchAllConsortiaAsync()
    {
        return await dataAccessProvider.GetAllConsortiaAsync();
    }
}