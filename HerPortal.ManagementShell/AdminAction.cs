using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public class AdminAction
{
    public enum UserStatus
    {
        New,
        Active
    }

    private readonly Dictionary<string, List<string>> consortiumCodeToCustodianCodesDict =
        ConsortiumData.ConsortiumCustodianCodesIdsByConsortiumCode;

    private readonly Dictionary<string, string> custodianCodeToConsortiumCodeDict =
        LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode;

    private readonly IDatabaseOperation dbOperation;

    public AdminAction(IDatabaseOperation dbOperation)
    {
        this.dbOperation = dbOperation;
    }

    public User? GetUser(string emailAddress)
    {
        var portalUsers = dbOperation.GetUsersWithLocalAuthoritiesAndConsortia();
        return
            portalUsers.SingleOrDefault(user => string.Equals
            (
                user.EmailAddress,
                emailAddress,
                StringComparison.CurrentCultureIgnoreCase
            ));
    }

    public bool CustodianCodeIsInOwnedConsortium(User user, string custodianCode)
    {
        var custodianCodesOfConsortia = user.Consortia
            .SelectMany(consortium =>
                consortiumCodeToCustodianCodesDict[consortium.ConsortiumCode]);
        return custodianCodesOfConsortia.Contains(custodianCode);
    }

    public List<string> GetOwnedCustodianCodesInConsortia(User user, IEnumerable<string> consortiumCodes)
    {
        return user.LocalAuthorities
            .Where(localAuthority =>
                consortiumCodes.Contains(custodianCodeToConsortiumCodeDict[localAuthority.CustodianCode]))
            .Select(localAuthority => localAuthority.CustodianCode)
            .ToList();
    }

    public UserStatus GetUserStatus(User? userOrNull)
    {
        return userOrNull == null ? UserStatus.New : UserStatus.Active;
    }

    public void CreateUser(string userEmailAddress, IReadOnlyCollection<string>? custodianCodes,
        IReadOnlyCollection<string>? consortiumCodes)
    {
        var lasToAdd = dbOperation.GetLas(custodianCodes ?? Array.Empty<string>());
        var consortiaToAdd = dbOperation.GetConsortia(consortiumCodes ?? Array.Empty<string>());

        dbOperation.CreateUserOrLogError(userEmailAddress, lasToAdd, consortiaToAdd);
    }

    public void RemoveUser(User user)
    {
        dbOperation.RemoveUserOrLogError(user);
    }

    public void AddLas(User user, IEnumerable<string> custodianCodes)
    {
        var filteredCustodianCodes = custodianCodes
            .Where(custodianCode => !CustodianCodeIsInOwnedConsortium(user, custodianCode))
            .ToList();

        var lasToAdd = dbOperation.GetLas(filteredCustodianCodes);

        dbOperation.AddLasToUser(user, lasToAdd);
    }

    public void RemoveLas(User user, IReadOnlyCollection<string> custodianCodes)
    {
        var lasToRemove = user.LocalAuthorities.Where(la => custodianCodes.Contains(la.CustodianCode)).ToList();
        var missingCodes = custodianCodes.Where(code => !lasToRemove.Any(la => la.CustodianCode.Equals(code))).ToList();
        if (missingCodes.Count > 0)
            throw new KeyNotFoundException(
                $"Could not find LAs attached to {user.EmailAddress} for the following codes: {string.Join(", ", missingCodes)}. Please check your inputs and try again.");

        dbOperation.RemoveLasFromUser(user, lasToRemove);
    }

    public void AddConsortia(User user, IReadOnlyCollection<string> consortiumCodes)
    {
        var consortiaToAdd = dbOperation.GetConsortia(consortiumCodes);

        var ownedCustodianCodesInConsortia = GetOwnedCustodianCodesInConsortia(user, consortiumCodes);
        var lasToRemove = dbOperation.GetLas(ownedCustodianCodesInConsortia);

        if (lasToRemove.Count > 0)
            dbOperation.AddConsortiaAndRemoveLasFromUser(user, consortiaToAdd, lasToRemove);
        else
            dbOperation.AddConsortiaToUser(user, consortiaToAdd);
    }

    public void RemoveConsortia(User user, IReadOnlyCollection<string> consortiumCodes)
    {
        var consortiaToRemove = user.Consortia.Where(consortium => consortiumCodes.Contains(consortium.ConsortiumCode))
            .ToList();
        var missingCodes = consortiumCodes
            .Where(code => !consortiaToRemove.Any(consortium => consortium.ConsortiumCode.Equals(code))).ToList();
        if (missingCodes.Count > 0)
            throw new KeyNotFoundException(
                $"Could not find Consortia attached to {user.EmailAddress} for the following codes: {string.Join(", ", missingCodes)}. Please check your inputs and try again.");

        dbOperation.RemoveConsortiaFromUser(user, consortiaToRemove);
    }
}