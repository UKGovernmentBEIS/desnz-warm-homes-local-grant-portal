using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public class CommandHandler
{
    private readonly AdminAction adminAction;

    private readonly Dictionary<string, string> consortiumCodeToConsortiumNameDict =
        ConsortiumData.ConsortiumNamesByConsortiumCode;

    private readonly Dictionary<string, string> custodianCodeToConsortiumNameDict =
        LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode
            .ToDictionary(
                kvp => kvp.Key,
                kvp => ConsortiumData.ConsortiumNamesByConsortiumCode[kvp.Value]);

    private readonly Dictionary<string, string> custodianCodeToLaNameDict =
        LocalAuthorityData.LocalAuthorityNamesByCustodianCode;

    private readonly IOutputProvider outputProvider;

    public CommandHandler(AdminAction adminAction, IOutputProvider outputProvider)
    {
        this.adminAction = adminAction;
        this.outputProvider = outputProvider;
    }

    public User? GetUser(string emailAddress)
    {
        return adminAction.GetUser(emailAddress);
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

        adminAction.RemoveUser(user);
    }

    public void CreateOrUpdateUserWithLas(string userEmailAddress, IReadOnlyCollection<string> custodianCodes)
    {
        var (user, userStatus) = CheckUserStatus(userEmailAddress, "LAs");

        var confirmation = ConfirmAddCustodianCodes(userEmailAddress, custodianCodes, user);

        if (confirmation)
            switch (userStatus)
            {
                case UserAccountStatus.Active:
                    TryAddLas(user, custodianCodes);
                    break;
                case UserAccountStatus.New:
                    TryCreateUser(userEmailAddress, custodianCodes, null);
                    break;
            }
    }

    public void TryRemoveLas(User? user, IReadOnlyCollection<string> custodianCodes)
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

        try
        {
            adminAction.RemoveLas(user, custodianCodes);
        }
        catch (CouldNotFindAuthorityException couldNotFindAuthorityException)
        {
            OutputCouldNotFindAuthorityException($"Could not remove Custodian Codes from {user.EmailAddress}.",
                couldNotFindAuthorityException);
        }
    }

    public void CreateOrUpdateUserWithConsortia(string userEmailAddress, IReadOnlyCollection<string> consortiumCodes)
    {
        var (user, userStatus) = CheckUserStatus(userEmailAddress, "Consortia");

        var confirmation = ConfirmAddConsortiumCodes(userEmailAddress, consortiumCodes, user);

        if (confirmation)
            switch (userStatus)
            {
                case UserAccountStatus.Active:
                    TryAddConsortia(user, consortiumCodes);
                    break;
                case UserAccountStatus.New:
                    TryCreateUser(userEmailAddress, null, consortiumCodes);
                    break;
            }
    }

    public void TryRemoveConsortia(User? user, IReadOnlyCollection<string> consortiumCodes)
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

        try
        {
            adminAction.RemoveConsortia(user, consortiumCodes);
        }
        catch (CouldNotFindAuthorityException couldNotFindAuthorityException)
        {
            OutputCouldNotFindAuthorityException($"Could not remove Consortium Codes from {user.EmailAddress}.",
                couldNotFindAuthorityException);
        }
    }

    public void FixAllUserOwnedConsortia()
    {
        outputProvider.Output("!!! User Migration Script !!!");
        outputProvider.Output("This script will ensure the validity of the LA / Consortium relationship for users.");
        outputProvider.Output(
            "If a user owns all LAs in a Consortium, they will be made a Consortium Admin and the LAs will be removed.");

        var users = adminAction.GetUsers();

        foreach (var user in users)
        {
            outputProvider.Output($"Processing user {user.EmailAddress}...");
            var consortiumCodesUserShouldOwn = adminAction.GetConsortiumCodesUserShouldOwn(user).ToList();

            if (consortiumCodesUserShouldOwn.Count == 0)
            {
                outputProvider.Output("No changes needed.");
                continue;
            }

            outputProvider.Output("This user should own the following Consortia:");
            PrintCodes(consortiumCodesUserShouldOwn,
                consortiumCode => consortiumCodeToConsortiumNameDict[consortiumCode]);

            var custodianCodesToRemove =
                adminAction.GetCustodianCodesInConsortia(consortiumCodesUserShouldOwn).ToList();
            outputProvider.Output(
                "To make this user a Consortium Admin, the following LAs will be removed from the user:");
            PrintCodes(custodianCodesToRemove, custodianCode => custodianCodeToLaNameDict[custodianCode]);

            var confirmation = outputProvider.Confirm("Okay to proceed? (Y/N)");

            if (confirmation)
                adminAction.FixUserOwnedConsortia(user);
            else
                outputProvider.Output("No changes made.");
        }

        outputProvider.Output("Migration complete.");
    }

    public void AddAllMissingAuthoritiesToDatabase()
    {
        outputProvider.Output("!!! Database Local Authority Population Script !!!");
        outputProvider.Output(
            "This script will ensure the database has an entry for every Local Authority & Consortium present in LocalAuthorityData or ConsortiumData.");
        outputProvider.Output("Use after adding a new Local Authority or Consortium to the code.");

        var custodianCodesMissingFromDatabase = adminAction.GetCustodianCodesMissingFromDatabase().ToList();
        var consortiumCodesMissingFromDatabase = adminAction.GetConsortiumCodesMissingFromDatabase().ToList();

        if (custodianCodesMissingFromDatabase.Count == 0 && consortiumCodesMissingFromDatabase.Count == 0)
        {
            outputProvider.Output("No changes needed.");
            return;
        }

        if (custodianCodesMissingFromDatabase.Count > 0)
        {
            outputProvider.Output("The following Local Authorities will be added to the database:");
            PrintCodes(custodianCodesMissingFromDatabase, code => custodianCodeToLaNameDict[code]);
        }

        if (consortiumCodesMissingFromDatabase.Count > 0)
        {
            outputProvider.Output("The following Consortia will be added to the database:");
            PrintCodes(consortiumCodesMissingFromDatabase, code => consortiumCodeToConsortiumNameDict[code]);
        }

        var confirmation = outputProvider.Confirm("Okay to proceed? (Y/N)");

        if (confirmation)
            adminAction.AddMissingAuthoritiesToDatabase();
        else
            outputProvider.Output("No changes made.");
    }

    private void OutputCouldNotFindAuthorityException(string wrapperMessage,
        CouldNotFindAuthorityException couldNotFindAuthorityException)
    {
        outputProvider.Output("!!! Error occured during operation !!!");
        outputProvider.Output(wrapperMessage);
        outputProvider.Output($"Invalid Codes: {string.Join(", ", couldNotFindAuthorityException.InvalidCodes)}");
        outputProvider.Output(couldNotFindAuthorityException.Message);
        outputProvider.Output("No data has been changed.");
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

    private bool ConfirmAddCustodianCodes(string userEmailAddress, IReadOnlyCollection<string> custodianCodes,
        User? user)
    {
        return ConfirmChangesToDatabase(userEmailAddress, () =>
        {
            if (user != null)
            {
                // flag the need to not add LAs that are in consortia the user owns
                var custodianCodeInOwnedConsortiumGrouping = custodianCodes.ToLookup(custodianCode =>
                    adminAction.CustodianCodeIsInOwnedConsortium(user, custodianCode));
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
                var ownedCustodianCodesInConsortia =
                    adminAction.GetOwnedCustodianCodesInConsortia(user, consortiumCodes);

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

    private void DisplayUserStatus(UserAccountStatus userAccountStatus, string authorityType)
    {
        switch (userAccountStatus)
        {
            case UserAccountStatus.New:
                outputProvider.Output("User not found in database. A new user will be created");
                break;
            case UserAccountStatus.Active:
                outputProvider.Output($"User found in database. {authorityType} will be added to their account");
                break;
        }
    }

    private void TryCreateUser(string userEmailAddress, IReadOnlyCollection<string>? custodianCodes,
        IReadOnlyCollection<string>? consortiumCodes)
    {
        try
        {
            adminAction.CreateUser(userEmailAddress, custodianCodes, consortiumCodes);
        }
        catch (CouldNotFindAuthorityException couldNotFindAuthorityException)
        {
            OutputCouldNotFindAuthorityException($"Could not create user {userEmailAddress}.",
                couldNotFindAuthorityException);
        }
    }

    private void TryAddLas(User? user, IReadOnlyCollection<string> custodianCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (custodianCodes.Count < 1)
        {
            outputProvider.Output("Please specify custodian codes to add to user");
            return;
        }

        try
        {
            adminAction.AddLas(user, custodianCodes);
        }
        catch (CouldNotFindAuthorityException couldNotFindAuthorityException)
        {
            OutputCouldNotFindAuthorityException($"Could not add Custodian Codes to {user.EmailAddress}.",
                couldNotFindAuthorityException);
        }
    }

    private void TryAddConsortia(User? user, IReadOnlyCollection<string> consortiumCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (consortiumCodes.Count < 1)
        {
            outputProvider.Output("Please specify consortium codes to add to user");
            return;
        }

        try
        {
            adminAction.AddConsortia(user, consortiumCodes);
        }
        catch (CouldNotFindAuthorityException couldNotFindAuthorityException)
        {
            OutputCouldNotFindAuthorityException($"Could not add Consortium Codes to {user.EmailAddress}.",
                couldNotFindAuthorityException);
        }
    }

    private (User? user, UserAccountStatus userStatus) CheckUserStatus(string userEmailAddress, string authorityType)
    {
        var user = adminAction.GetUser(userEmailAddress);
        var userStatus = adminAction.GetUserStatus(user);
        DisplayUserStatus(userStatus, authorityType);
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
}