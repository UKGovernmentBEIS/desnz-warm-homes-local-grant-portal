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

    public async Task<IList<User>> GetAllDeliveryPartnersAsync()
    {
        return await dataAccessProvider.GetAllDeliveryPartnersAsync();
    }

    public async Task<IEnumerable<User>> GetAllActiveDeliveryPartnersAsync()
    {
        return await dataAccessProvider.GetAllActiveDeliveryPartnersAsync();
    }

    public async Task<IList<User>> GetAllDeliveryPartnersWhereEmailContainsAsync(string partialEmailAddress)
    {
        return await dataAccessProvider.GetAllDeliveryPartnersWhereEmailContainsAsync(partialEmailAddress);
    }
}