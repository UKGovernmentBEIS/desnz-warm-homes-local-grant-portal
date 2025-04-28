using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;

namespace WhlgPortalWebsite.ManagementShell;

public interface IDatabaseOperation
{
    public List<User> GetUsersWithLocalAuthoritiesAndConsortia();
    public List<LocalAuthority> GetLas(IReadOnlyCollection<string> custodianCodes);
    public List<Consortium> GetConsortia(IReadOnlyCollection<string> consortiumCodes);
    public void RemoveUserOrLogError(User user);

    public void CreateUserOrLogError(string userEmailAddress, UserRole userRole, List<LocalAuthority> localAuthorities,
        List<Consortium> consortia);

    public void AddLasToUser(User user, List<LocalAuthority> localAuthorities);
    public void AddConsortiaToUser(User user, List<Consortium> consortia);
    public void RemoveLasFromUser(User user, List<LocalAuthority> localAuthorities);

    public void AddConsortiaAndRemoveLasFromUser(User user, List<Consortium> consortia,
        List<LocalAuthority> localAuthorities);

    public void RemoveConsortiaFromUser(User user, List<Consortium> consortia);
    public IEnumerable<LocalAuthority> GetAllLas();
    public IEnumerable<Consortium> GetAllConsortia();
    public void CreateLasAndConsortia(IEnumerable<string> custodianCodes, IEnumerable<string> consortiumCodes);
}