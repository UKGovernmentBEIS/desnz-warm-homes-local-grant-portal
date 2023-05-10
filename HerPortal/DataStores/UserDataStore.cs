using System.Threading.Tasks;
using HerPortal.BusinessLogic.Models;
using HerPortal.Data;
using Microsoft.Extensions.Logging;

namespace HerPortal.DataStores;

public class UserDataStore
{
    private readonly IDataAccessProvider dataAccessProvider;
    private readonly ILogger<UserDataStore> logger;

    public UserDataStore(IDataAccessProvider dataAccessProvider, ILogger<UserDataStore> logger)
    {
        this.dataAccessProvider = dataAccessProvider;
        this.logger = logger;
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