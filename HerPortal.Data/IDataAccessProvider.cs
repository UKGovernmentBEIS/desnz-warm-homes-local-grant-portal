using HerPortal.BusinessLogic.Models;

namespace HerPortal.Data;

public interface IDataAccessProvider
{
    public Task<User> GetUserByEmailAsync(string emailAddress);
    public Task MarkUserAsHavingLoggedInAsync(int userId);
}
