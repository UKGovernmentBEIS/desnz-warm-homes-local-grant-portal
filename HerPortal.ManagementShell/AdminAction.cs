using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public class AdminAction
{
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
            .Where(localAuthority => custodianCodeToConsortiumCodeDict.ContainsKey(localAuthority.CustodianCode) &&
                                     consortiumCodes.Contains(
                                         custodianCodeToConsortiumCodeDict[localAuthority.CustodianCode]))
            .Select(localAuthority => localAuthority.CustodianCode)
            .ToList();
    }

    public IEnumerable<string> GetCustodianCodesInConsortia(IEnumerable<string> consortiumCodes)
    {
        return consortiumCodes.SelectMany(consortiumCode => consortiumCodeToCustodianCodesDict[consortiumCode]);
    }

    public UserAccountStatus GetUserStatus(User? userOrNull)
    {
        return userOrNull == null ? UserAccountStatus.New : UserAccountStatus.Active;
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
        var missingCodes = custodianCodes.Where(code =>
                !lasToRemove
                    .Select(la => la.CustodianCode)
                    .Contains(code))
            .ToList();
        if (missingCodes.Count > 0)
            throw new CouldNotFindAuthorityException("Custodian Codes are not associated with this user.",
                missingCodes);

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
            .Where(code => !consortiaToRemove
                .Select(consortium => consortium.ConsortiumCode)
                .Contains(code)
            ).ToList();
        if (missingCodes.Count > 0)
            throw new CouldNotFindAuthorityException("Consortium Codes are not associated with this user.",
                missingCodes);

        dbOperation.RemoveConsortiaFromUser(user, consortiaToRemove);
    }

    public List<User> GetUsers()
    {
        return dbOperation.GetUsersWithLocalAuthoritiesAndConsortia();
    }

    public void FixUserOwnedConsortia(User user)
    {
        var consortiumCodesToAdd = GetConsortiumCodesUserShouldOwn(user).ToList();
        var custodianCodesToRemove = GetCustodianCodesInConsortia(consortiumCodesToAdd).ToList();

        var consortiaToAdd = dbOperation.GetConsortia(consortiumCodesToAdd);
        var lasToRemove = dbOperation.GetLas(custodianCodesToRemove);

        dbOperation.AddConsortiaAndRemoveLasFromUser(user, consortiaToAdd, lasToRemove);
    }

    public IEnumerable<string> GetConsortiumCodesUserShouldOwn(User user)
    {
        var userCustodianCodes = user
            .LocalAuthorities
            .Select(la => la.CustodianCode)
            .ToList();

        return ConsortiumData.ConsortiumCustodianCodesIdsByConsortiumCode
            .Where(kvp =>
                kvp.Value.All(custodianCode => userCustodianCodes.Contains(custodianCode)))
            .Select(kvp => kvp.Key);
    }

    public void AddMissingAuthoritiesToDatabase()
    {
        var custodianCodesMissingFromDatabase = GetCustodianCodesMissingFromDatabase().ToList();
        var consortiumCodesMissingFromDatabase = GetConsortiumCodesMissingFromDatabase().ToList();

        dbOperation.CreateLasAndConsortia(custodianCodesMissingFromDatabase, consortiumCodesMissingFromDatabase);
    }

    public IEnumerable<string> GetCustodianCodesMissingFromDatabase()
    {
        var custodianCodesInCode = LocalAuthorityData.LocalAuthorityNamesByCustodianCode.Keys;
        var custodianCodesInDatabase = dbOperation.GetAllLas().Select(la => la.CustodianCode);

        return custodianCodesInCode.Except(custodianCodesInDatabase);
    }

    public IEnumerable<string> GetConsortiumCodesMissingFromDatabase()
    {
        var consortiumCodesInCode = ConsortiumData.ConsortiumNamesByConsortiumCode.Keys;
        var consortiumCodesInDatabase = dbOperation.GetAllConsortia().Select(consortia => consortia.ConsortiumCode);

        return consortiumCodesInCode.Except(consortiumCodesInDatabase);
    }
}