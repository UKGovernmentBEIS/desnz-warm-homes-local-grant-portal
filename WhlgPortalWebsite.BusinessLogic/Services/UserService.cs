using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services;

public interface IUserService
{
    Task<User> GetUserByEmailAsync(string email);
    Task<User> GetUserByIdAsync(int userId);
    Task MarkUserAsHavingLoggedInAsync(int userId);
    Task<IEnumerable<User>> GetAllActiveDeliveryPartnersAsync();
    Task<IEnumerable<User>> SearchAllDeliveryPartnersAsync(string searchEmailAddress);
    Task<User> CreateDeliveryPartnerAsync(string emailAddress);
    Task<bool> IsEmailAddressInUseAsync(string emailAddress);

    Task AssignCodesToDeliveryPartnerAsync(User user, string codeToBeAssigned);

    IEnumerable<string> SearchAllLocalAuthoritiesAsync(string searchTerm);
    IEnumerable<string> SearchAllConsortiaAsync(string searchTerm);
}

public class UserService(IDataAccessProvider dataAccessProvider) : IUserService
{
    public async Task<User> GetUserByEmailAsync(string emailAddress)
    {
        try
        {
            return await dataAccessProvider.GetUserByEmailAsync(emailAddress);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException("User not found.", ex);
        }
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        try
        {
            return await dataAccessProvider.GetUserByIdAsync(userId);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException("User not found.", ex);
        }
    }

    public async Task MarkUserAsHavingLoggedInAsync(int userId)
    {
        await dataAccessProvider.MarkUserAsHavingLoggedInAsync(userId);
    }

    public async Task<IEnumerable<User>> GetAllActiveDeliveryPartnersAsync()
    {
        return await dataAccessProvider.GetAllActiveDeliveryPartnersAsync();
    }

    /// <summary>
    /// Search all users in the database with an email that includes the search term. Matches case-insensitively
    /// </summary>
    /// <param name="searchEmailAddress">Fragment of email to search with. Leave as blank or null to return all users</param>
    /// <returns>All matching users</returns>
    public async Task<IEnumerable<User>> SearchAllDeliveryPartnersAsync(string searchEmailAddress)
    {
        return string.IsNullOrWhiteSpace(searchEmailAddress) switch
        {
            true => await dataAccessProvider.GetAllDeliveryPartnersAsync(),
            false => await dataAccessProvider.GetAllDeliveryPartnersWhereEmailContainsAsync(searchEmailAddress)
        };
    }

    public async Task<User> CreateDeliveryPartnerAsync(string emailAddress)
    {
        return await dataAccessProvider.CreateDeliveryPartnerAsync(emailAddress);
    }

    public async Task<bool> IsEmailAddressInUseAsync(string emailAddress)
    {
        return await dataAccessProvider.IsEmailAddressInUseAsync(emailAddress);
    }

    public async Task AddLasToDeliveryPartnerAsync(User user, LocalAuthority localAuthority)
    {
        await dataAccessProvider.AddLaToDeliveryPartnerAsync(user, localAuthority);
    }

    public async Task AddConsortiaToDeliveryPartnerAsync(User user, Consortium consortium)
    {
        await dataAccessProvider.AddConsortiumToDeliveryPartnerAsync(user, consortium);
    }

    public IEnumerable<string> SearchAllLocalAuthoritiesAsync(string searchTerm)
    {
        List<string> matchingLaCodes = [];

        foreach (var kvp in LocalAuthorityData.LocalAuthorityNamesByCustodianCode)
        {
            if (kvp.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                matchingLaCodes.Add(kvp.Key);
            }
        }

        return matchingLaCodes;
    }

    public IEnumerable<string> SearchAllConsortiaAsync(string searchTerm)
    {
        List<string> matchingConsortiumCodes = [];

        foreach (var kvp in ConsortiumData.ConsortiumNamesByConsortiumCode)
        {
            if (kvp.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                matchingConsortiumCodes.Add(kvp.Key);
            }
        }

        return matchingConsortiumCodes;
    }

    public async Task AssignCodesToDeliveryPartnerAsync(User user, string codeToBeAssigned)
    {
        // TODO PC-1842: Improve this logic
        if (codeToBeAssigned.StartsWith("C"))
        {
            var consortium = await dataAccessProvider.GetConsortiumByConsortiumCodeAsync(codeToBeAssigned);
            await dataAccessProvider.AddConsortiumToDeliveryPartnerAsync(user, consortium);
        }
        else
        {
            var localAuthority = await dataAccessProvider.GetLocalAuthorityByCustodianCodeAsync(codeToBeAssigned);
            await dataAccessProvider.AddLaToDeliveryPartnerAsync(user, localAuthority);
        }
    }
}