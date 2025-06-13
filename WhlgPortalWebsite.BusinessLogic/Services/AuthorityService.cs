using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services;

public interface IAuthorityService
{
    Task<IEnumerable<LocalAuthority>> GetAllLasAsync();

    Task<IEnumerable<Consortium>> GetAllConsortiaAsync();
}

public class AuthorityService(IDataAccessProvider dataAccessProvider) : IAuthorityService
{
    public async Task<IEnumerable<LocalAuthority>> GetAllLasAsync()
    {
        return await dataAccessProvider.GetAllLasAsync();
    }

    public async Task<IEnumerable<Consortium>> GetAllConsortiaAsync()
    {
        return await dataAccessProvider.GetAllConsortiaAsync();
    }
}