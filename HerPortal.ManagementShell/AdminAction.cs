using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public class AdminAction
{
    public enum UserStatus
    {
        New,
        Active
    }

    private readonly Dictionary<string, string> consortiumCodeToConsortiumNameDict =
        ConsortiumData.ConsortiumNamesByConsortiumCode;

    private readonly Dictionary<string, List<string>> consortiumCodeToCustodianCodesDict =
        ConsortiumData.ConsortiumCustodianCodesIdsByConsortiumCode;

    private readonly Dictionary<string, string> custodianCodeToConsortiumCodeDict =
        LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode;

    private readonly Dictionary<string, string> custodianCodeToConsortiumNameDict =
        LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode
            .ToDictionary(
                kvp => kvp.Key,
                kvp => ConsortiumData.ConsortiumNamesByConsortiumCode[kvp.Value]);

    private readonly Dictionary<string, string> custodianCodeToLaNameDict =
        LocalAuthorityData.LocalAuthorityNamesByCustodianCode;

    private readonly IDatabaseOperation dbOperation;
    private readonly IOutputProvider outputProvider;

    public AdminAction(IDatabaseOperation dbOperation, IOutputProvider outputProvider)
    {
        this.dbOperation = dbOperation;
        this.outputProvider = outputProvider;
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

    private void PrintCodes(IReadOnlyCollection<string> codes, Func<string, string> codeToName)
    {
        if (codes.Count < 1) outputProvider.Output("(None)");

        foreach (var code in codes)
        {
            var name = codeToName(code);
            outputProvider.Output($"{code}: {name}");
        }
    }

    private bool ConfirmAddCustodianCodes(string userEmailAddress, IReadOnlyCollection<string> custodianCodes, User? user)
    {
        return ConfirmChangesToDatabase(userEmailAddress, () =>
        {
            if (user != null)
            {
                // flag the need to not add LAs that are in consortia the user owns
                var custodianCodeInOwnedConsortiumGrouping = custodianCodes.ToLookup(custodianCode =>
                    CustodianCodeIsInOwnedConsortium(user, custodianCode));
                var custodianCodesNotInOwnedConsortium = custodianCodeInOwnedConsortiumGrouping[false].ToList();
                var custodianCodesInOwnedConsortium = custodianCodeInOwnedConsortiumGrouping[true].ToList();

                outputProvider.Output("Add the following Local Authorities:");
                PrintCodes(custodianCodesNotInOwnedConsortium, code => custodianCodeToLaNameDict[code]);
                outputProvider.Output("Ignore the following Local Authorities already in owned Consortia:");
                PrintCodes(custodianCodesInOwnedConsortium, code =>
                    $"{custodianCodeToLaNameDict[code]} ({custodianCodeToConsortiumNameDict[code]})");
            }
            else
            {
                outputProvider.Output("Add the following Local Authorities:");
                PrintCodes(custodianCodes, code => custodianCodeToLaNameDict[code]);
            }
        });
    }

    private bool ConfirmRemoveCustodianCodes(string userEmailAddress, IReadOnlyCollection<string> custodianCodes)
    {
        return ConfirmChangesToDatabase(userEmailAddress, () =>
        {
            outputProvider.Output("Remove the following Local Authorities:");
            PrintCodes(custodianCodes, code => custodianCodeToLaNameDict[code]);
        });
    }

    private bool CustodianCodeIsInOwnedConsortium(User user, string custodianCode)
    {
        var custodianCodesOfConsortia = user.Consortia
            .SelectMany(consortium =>
                consortiumCodeToCustodianCodesDict[consortium.ConsortiumCode]);
        return custodianCodesOfConsortia.Contains(custodianCode);
    }

    private bool ConfirmAddConsortiumCodes(string? userEmailAddress, IReadOnlyCollection<string> consortiumCodes,
        User? user)
    {
        return ConfirmChangesToDatabase(userEmailAddress, () =>
        {
            if (user != null)
            {
                outputProvider.Output("Add the following Consortia:");
                PrintCodes(consortiumCodes, code => consortiumCodeToConsortiumNameDict[code]);

                // flag the need to remove access for any LAs in the new consortia
                var ownedCustodianCodesInConsortia = GetOwnedCustodianCodesInConsortia(user, consortiumCodes);

                outputProvider.Output("Remove the following Local Authorities in these Consortia:");
                PrintCodes(ownedCustodianCodesInConsortia, code =>
                    $"{custodianCodeToLaNameDict[code]} ({custodianCodeToConsortiumNameDict[code]})");
            }
            else
            {
                outputProvider.Output("Add the following Consortia:");
                PrintCodes(consortiumCodes, code => consortiumCodeToConsortiumNameDict[code]);
            }
        });
    }

    private bool ConfirmRemoveConsortiumCodes(string? userEmailAddress, IReadOnlyCollection<string> consortiumCodes)
    {
        return ConfirmChangesToDatabase(userEmailAddress, () =>
        {
            outputProvider.Output("Remove the following Consortia:");
            PrintCodes(consortiumCodes, code => consortiumCodeToConsortiumNameDict[code]);
        });
    }

    private bool ConfirmChangesToDatabase(string? userEmailAddress, Action printAction)
    {
        outputProvider.Output(
            $"You are changing permissions for user {userEmailAddress}:");

        try
        {
            printAction();
        }
        catch (Exception e)
        {
            outputProvider.Output($"{e.Message} Process terminated");
            return false;
        }

        var hasUserConfirmed = outputProvider.Confirm("Please confirm (y/n)");
        if (!hasUserConfirmed) outputProvider.Output("Process cancelled, no changes were made to the database");

        return hasUserConfirmed;
    }

    private List<string> GetOwnedCustodianCodesInConsortia(User user, IEnumerable<string> consortiumCodes)
    {
        return user.LocalAuthorities
            .Where(localAuthority =>
                consortiumCodes.Contains(custodianCodeToConsortiumCodeDict[localAuthority.CustodianCode]))
            .Select(localAuthority => localAuthority.CustodianCode)
            .ToList();
    }

    private void DisplayUserStatus(Enum status)
    {
        switch (status)
        {
            case UserStatus.New:
                outputProvider.Output("User not found in database. A new user will be created");
                break;
            case UserStatus.Active:
                outputProvider.Output("User found in database. LAs will be added to their account");
                break;
        }
    }

    private UserStatus GetUserStatus(User? userOrNull)
    {
        return userOrNull == null ? UserStatus.New : UserStatus.Active;
    }

    private void TryCreateUser(string userEmailAddress, IReadOnlyCollection<string>? custodianCodes,
        IReadOnlyCollection<string>? consortiumCodes)
    {
        try
        {
            var lasToAdd = dbOperation.GetLas(custodianCodes ?? Array.Empty<string>());
            var consortiaToAdd = dbOperation.GetConsortia(consortiumCodes ?? Array.Empty<string>());

            dbOperation.CreateUserOrLogError(userEmailAddress, lasToAdd, consortiaToAdd);
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            outputProvider.Output($"Could not create user: {keyNotFoundException.Message}");
        }
    }

    public void TryRemoveUser(User? user)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        var deletionConfirmation = outputProvider.Confirm(
            $"Attention! This will delete user {user.EmailAddress} and all associated rows from the database. Are you sure you want to commit this transaction? (y/n)");
        if (!deletionConfirmation) return;

        dbOperation.RemoveUserOrLogError(user);
    }

    private void AddLas(User? user, IReadOnlyCollection<string>? custodianCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (custodianCodes == null || custodianCodes.Count < 1)
        {
            outputProvider.Output("Please specify custodian codes to add to user");
            return;
        }

        try
        {
            var filteredCustodianCodes = custodianCodes
                .Where(custodianCode => !CustodianCodeIsInOwnedConsortium(user, custodianCode))
                .ToList();

            var lasToAdd = dbOperation.GetLas(filteredCustodianCodes);

            dbOperation.AddLasToUser(user, lasToAdd);
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            outputProvider.Output($"Could not add LAs to user: {keyNotFoundException.Message}");
        }
    }

    public void RemoveLas(User? user, IReadOnlyCollection<string> custodianCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (custodianCodes.Count == 0)
        {
            outputProvider.Output("Please specify custodian codes to remove from user");
            return;
        }

        var userConfirmation = ConfirmRemoveCustodianCodes(user.EmailAddress, custodianCodes);
        if (!userConfirmation) return;

        var lasToRemove = user.LocalAuthorities.Where(la => custodianCodes.Contains(la.CustodianCode)).ToList();
        var missingCodes = custodianCodes.Where(code => !lasToRemove.Any(la => la.CustodianCode.Equals(code))).ToList();
        if (missingCodes.Count > 0)
        {
            outputProvider.Output(
                $"Could not find LAs attached to {user.EmailAddress} for the following codes: {string.Join(", ", missingCodes)}. Please check your inputs and try again.");
            return;
        }

        dbOperation.RemoveLasFromUser(user, lasToRemove);
    }

    private void AddConsortia(User? user, IReadOnlyCollection<string>? consortiumCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (consortiumCodes == null || consortiumCodes.Count < 1)
        {
            outputProvider.Output("Please specify consortium codes to add to user");
            return;
        }

        try
        {
            var consortiaToAdd = dbOperation.GetConsortia(consortiumCodes);

            var ownedCustodianCodesInConsortia = GetOwnedCustodianCodesInConsortia(user, consortiumCodes);
            var lasToRemove = dbOperation.GetLas(ownedCustodianCodesInConsortia);

            if (lasToRemove.Count > 0)
                dbOperation.AddConsortiaAndRemoveLasFromUser(user, consortiaToAdd, lasToRemove);
            else
                dbOperation.AddConsortiaToUser(user, consortiaToAdd);
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            outputProvider.Output($"Could not add Consortia to user: {keyNotFoundException.Message}");
        }
    }

    private (User? user, UserStatus userStatus) CheckUserStatus(string userEmailAddress)
    {
        var user = GetUser(userEmailAddress);
        var userStatus = GetUserStatus(user);
        DisplayUserStatus(userStatus);
        outputProvider.Output("");

        outputProvider.Output("!!! ATTENTION! READ CAREFULLY OR RISK A DATA BREACH !!!");
        outputProvider.Output("");
        outputProvider.Output(
            "You are about to grant a user permission to read PERSONALLY IDENTIFIABLE INFORMATION submitted to LAs.");
        outputProvider.Output(
            "Take a moment to double check the following list and only continue if you are certain this user should have access to these LAs.");
        outputProvider.Output(
            "NB: in particular, you should only do this for LAs that have signed their DSA contracts!");
        outputProvider.Output("");

        return (user, userStatus);
    }

    public void CreateOrUpdateUserWithLas(string userEmailAddress, IReadOnlyCollection<string> custodianCodes)
    {
        var (user, userStatus) = CheckUserStatus(userEmailAddress);

        var confirmation = ConfirmAddCustodianCodes(userEmailAddress, custodianCodes, user);

        if (confirmation)
            switch (userStatus)
            {
                case UserStatus.Active:
                    AddLas(user, custodianCodes);
                    break;
                case UserStatus.New:
                    TryCreateUser(userEmailAddress, custodianCodes, null);
                    break;
            }
    }

    public void CreateOrUpdateUserWithConsortia(string userEmailAddress, IReadOnlyCollection<string> consortiumCodes)
    {
        var (user, userStatus) = CheckUserStatus(userEmailAddress);

        var confirmation = ConfirmAddConsortiumCodes(userEmailAddress, consortiumCodes, user);

        if (confirmation)
            switch (userStatus)
            {
                case UserStatus.Active:
                    AddConsortia(user, consortiumCodes);
                    break;
                case UserStatus.New:
                    TryCreateUser(userEmailAddress, null, consortiumCodes);
                    break;
            }
    }

    public void RemoveConsortia(User? user, IReadOnlyCollection<string> consortiumCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (consortiumCodes.Count == 0)
        {
            outputProvider.Output("Please specify consortium codes to remove from user");
            return;
        }

        var userConfirmation = ConfirmRemoveConsortiumCodes(user.EmailAddress, consortiumCodes);
        if (!userConfirmation) return;

        var consortiaToRemove = user.Consortia.Where(consortium => consortiumCodes.Contains(consortium.ConsortiumCode))
            .ToList();
        var missingCodes = consortiumCodes
            .Where(code => !consortiaToRemove.Any(consortium => consortium.ConsortiumCode.Equals(code))).ToList();
        if (missingCodes.Count > 0)
        {
            outputProvider.Output(
                $"Could not find Consortia attached to {user.EmailAddress} for the following codes: {string.Join(", ", missingCodes)}. Please check your inputs and try again.");
            return;
        }

        dbOperation.RemoveConsortiaFromUser(user, consortiaToRemove);
    }
}