using System.Threading.Tasks;
using HerPortal.BusinessLogic.Models;
using HerPortal.Data;

namespace HerPortal.DataStores;

public class UserDataStore
{
    private readonly IDataAccessProvider dataAccessProvider;

    public UserDataStore(IDataAccessProvider dataAccessProvider)
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
}
