using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public interface IDatabaseOperation
{
    public List<User> GetUsersWithLocalAuthorities();
    public List<LocalAuthority> GetLas(string[] custodianCodes);
    public List<Consortium> GetConsortia(string[] consortiumCodes);
    public void RemoveUserOrLogError(User user);
    public void CreateUserOrLogError(string userEmailAddress, List<LocalAuthority> localAuthorities, List<Consortium> consortia);
    public void AddLasToUser(User user, List<LocalAuthority> localAuthorities);
    public void RemoveLasFromUser(User user, List<LocalAuthority> localAuthorities);
}