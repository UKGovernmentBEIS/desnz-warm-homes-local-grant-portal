using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services;

public interface IUserService
{
    Task<User> GetUserByEmailAsync(string email);
    Task MarkUserAsHavingLoggedInAsync(int userId);
    Task<IEnumerable<User>> GetAllActiveDeliveryPartnersAsync();
    Task<IEnumerable<User>> SearchAllDeliveryPartnersAsync(string searchEmailAddress);
    Task CreateDeliveryPartnerAsync(string emailAddress);
    Task<bool> IsEmailAddressInUseAsync(string emailAddress);
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

    public async Task CreateDeliveryPartnerAsync(string emailAddress)
    {
        await dataAccessProvider.CreateDeliveryPartnerAsync(emailAddress);
    }

    public async Task<bool> IsEmailAddressInUseAsync(string emailAddress)
    {
        return await dataAccessProvider.IsEmailAddressInUseAsync(emailAddress);
    }
}