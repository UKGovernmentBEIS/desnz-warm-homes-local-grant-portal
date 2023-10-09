using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public interface IDatabaseOperation
{
    public List<User> GetUsersWithLocalAuthorities();
    public void RemoveUserOrLogError(User? user);
    public void CreateUserOrLogError(string userEmailAddress, string[]? custodianCodes);
    public void AddLasToUser(string[]? custodianCodes, User? user);
    public void RemoveLasFromUser(string[]? custodianCodes, User? user);
}