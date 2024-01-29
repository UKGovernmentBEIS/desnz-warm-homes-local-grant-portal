using HerPortal.BusinessLogic.Models;

namespace HerPortal.BusinessLogic.Services;

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

    public List<string> GetConsortiumIdsForUser(User user)
    {
        var userLocalAuthorities = user.LocalAuthorities.Select(la => la.CustodianCode);

        // user is a consortium manager if they are a manager of all LAs in that consortium
        return ConsortiumData.ConsortiumLocalAuthorityIdsByConsortiumId
            .Where(pair => pair.Value.All(consortiumLa => userLocalAuthorities.Contains(consortiumLa)))
            .Select(pair => pair.Key)
            .ToList();
    }
}
