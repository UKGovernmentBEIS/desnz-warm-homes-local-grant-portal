using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services;

public class UserService
{
    private readonly IDataAccessProvider dataAccessProvider;

    public UserService(IDataAccessProvider dataAccessProvider)
    {
        this.dataAccessProvider = dataAccessProvider;
    }

    public async Task<User> GetUserByEmailAsync(string emailAddress)
    {
        var user = await dataAccessProvider.GetUserByEmailAsync(emailAddress);

        return user;
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
}