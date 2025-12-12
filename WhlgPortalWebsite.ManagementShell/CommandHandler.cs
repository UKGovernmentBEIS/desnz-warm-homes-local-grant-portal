using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;

namespace WhlgPortalWebsite.ManagementShell;

public class CommandHandler(AdminAction adminAction, IOutputProvider outputProvider)
{
    private readonly Dictionary<string, string> consortiumCodeToConsortiumNameDict =
        ConsortiumData.ConsortiumNamesByConsortiumCode;

    private readonly Dictionary<string, string> custodianCodeToConsortiumNameDict =
        LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode
            .ToDictionary(
                kvp => kvp.Key,
                kvp => ConsortiumData.ConsortiumNamesByConsortiumCode[kvp.Value]);

    private readonly Dictionary<string, string> custodianCodeToLaNameDict =
        LocalAuthorityData.LocalAuthorityNamesByCustodianCode;

    public User? GetUser(string emailAddress)
    {
        return adminAction.GetUser(emailAddress);
    }

    public void TryRemoveUser(string userEmailAddress)
    {
        var user = adminAction.GetUser(userEmailAddress);
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
        var (user, userStatus) = GetUserAndStatus(userEmailAddress, UserRole.DeliveryPartner);

        DisplayDeliveryPartnerStatusAndWarning(userStatus, "LAs");

        if (userStatus is UserAccountStatus.IncorrectRole) return;

        var confirmation = ConfirmAddCustodianCodes(userEmailAddress, custodianCodes, user);

        if (confirmation)
            switch (userStatus)
            {
                case UserAccountStatus.Active:
                    TryAddLas(user, custodianCodes);
                    break;
                case UserAccountStatus.New:
                    TryCreateUser(userEmailAddress, UserRole.DeliveryPartner, custodianCodes, null);
                    break;
            }
    }

    public void TryRemoveLas(string userEmailAddress, IReadOnlyCollection<string> custodianCodes)
    {
        var (user, userStatus) = GetUserAndStatus(userEmailAddress, UserRole.DeliveryPartner);

        if (userStatus is UserAccountStatus.IncorrectRole)
        {
            DisplayIncorrectRoleError(UserRole.DeliveryPartner);
            return;
        }

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
        var (user, userStatus) = GetUserAndStatus(userEmailAddress, UserRole.DeliveryPartner);

        DisplayDeliveryPartnerStatusAndWarning(userStatus, "Consortia");

        if (userStatus is UserAccountStatus.IncorrectRole) return;

        var confirmation = ConfirmAddConsortiumCodes(userEmailAddress, consortiumCodes, user);

        if (confirmation)
            switch (userStatus)
            {
                case UserAccountStatus.Active:
                    TryAddConsortia(user, consortiumCodes);
                    break;
                case UserAccountStatus.New:
                    TryCreateUser(userEmailAddress, UserRole.DeliveryPartner, null, consortiumCodes);
                    break;
            }
    }

    public void TryRemoveConsortia(string userEmailAddress, IReadOnlyCollection<string> consortiumCodes)
    {
        var (user, userStatus) = GetUserAndStatus(userEmailAddress, UserRole.DeliveryPartner);

        if (userStatus is UserAccountStatus.IncorrectRole)
        {
            DisplayIncorrectRoleError(UserRole.DeliveryPartner);
            return;
        }

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

    public void TryAddServiceManager(string userEmailAddress)
    {
        var (_, userStatus) = GetUserAndStatus(userEmailAddress, UserRole.ServiceManager);

        DisplayServiceManagerStatusAndWarning(userStatus);

        if (userStatus is UserAccountStatus.Active or UserAccountStatus.IncorrectRole) return;

        var confirmation = outputProvider.Confirm(
            $"Are you sure you want to make {userEmailAddress} a Service Manager? (y/n)");

        if (confirmation)
        {
            TryCreateUser(userEmailAddress, UserRole.ServiceManager, null, null);
        }
    }

    public void FixAllUserOwnedConsortia()
    {
        outputProvider.Output("!!! User Migration Script !!!");
        outputProvider.Output("This script will ensure the validity of the LA / Consortium relationship for users.");
        outputProvider.Output(
            "If a user owns all LAs in a Consortium, they will be made a Consortium Admin and the LAs will be removed.");

        var users = adminAction.GetUsersIncludingLocalAuthoritiesAndConsortia();

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

    private void DisplayDeliveryPartnerStatusAndWarning(UserAccountStatus userAccountStatus, string authorityType)
    {
        switch (userAccountStatus)
        {
            case UserAccountStatus.New:
                outputProvider.Output("User not found in database. A new user will be created.");
                break;
            case UserAccountStatus.Active:
                outputProvider.Output($"User found in database. {authorityType} will be added to their account.");
                break;
            case UserAccountStatus.IncorrectRole:
                DisplayIncorrectRoleError(UserRole.DeliveryPartner);
                return;
        }

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
    }

    private void DisplayServiceManagerStatusAndWarning(UserAccountStatus userAccountStatus)
    {
        switch (userAccountStatus)
        {
            case UserAccountStatus.New:
                outputProvider.Output("User not found in database. A new user will be created.");
                outputProvider.Output("");
                outputProvider.Output("!!! ATTENTION! READ CAREFULLY OR RISK A DATA BREACH !!!");
                outputProvider.Output("");
                outputProvider.Output(
                    "You are about to give this user permission to grant Delivery Partner level access to other users.");
                outputProvider.Output(
                    "Take a moment to double check and only continue if you are certain this user should have Service Manager level access.");
                outputProvider.Output("");
                break;
            case UserAccountStatus.Active:
                outputProvider.Output(
                    "A Service Manager user is already associated with this email address in the database. No changes have been made to their account.");
                break;
            case UserAccountStatus.IncorrectRole:
                DisplayIncorrectRoleError(UserRole.ServiceManager);
                return;
        }
    }

    private void DisplayIncorrectRoleError(UserRole proposedUserRole)
    {
        var proposedUserRoleString = proposedUserRole switch
        {
            UserRole.DeliveryPartner => "Delivery Partner",
            UserRole.ServiceManager => "Service Manager",
            _ => throw new ArgumentOutOfRangeException(nameof(proposedUserRole), proposedUserRole, null)
        };

        outputProvider.Output(
            $"This email address is associated with a user that is not a {proposedUserRoleString}. Check the database & documentation to ensure the correct command is being executed.");
    }

    private void TryCreateUser(string userEmailAddress, UserRole userRole, IReadOnlyCollection<string>? custodianCodes,
        IReadOnlyCollection<string>? consortiumCodes)
    {
        try
        {
            adminAction.CreateUser(userEmailAddress, userRole, custodianCodes, consortiumCodes);
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

    private (User? user, UserAccountStatus userStatus) GetUserAndStatus(string userEmailAddress,
        UserRole proposedUserRole)
    {
        var user = adminAction.GetUser(userEmailAddress);
        var userStatus = adminAction.GetUserStatus(user, proposedUserRole);

        return (user, userStatus);
    }

    // Maintenance mode methods

    public void TrySetMaintenanceState(string[] args)
    {
        DisplayMaintenanceStateWarnings();

        EmergencyMaintenanceState? argEmergencyMaintenanceState = ParseEmergencyMaintenanceStateInput(args);
        if (argEmergencyMaintenanceState is null) return;

        var emergencyMaintenanceVerb =
            argEmergencyMaintenanceState == EmergencyMaintenanceState.Enabled ? "ENABLE" : "DISABLE";
        var liveEmergencyMaintenanceState = adminAction.GetEmergencyMaintenanceState();

        DisplayMaintenanceStateDetails(emergencyMaintenanceVerb, liveEmergencyMaintenanceState);

        if (argEmergencyMaintenanceState == liveEmergencyMaintenanceState)
        {
            outputProvider.Output("The requested state is the same as the current state.");
            outputProvider.Output("Exiting without changes...");
            return;
        }

        var confirmation = GetUserConfirmationForSettingMaintenanceState(emergencyMaintenanceVerb);
        if (!confirmation) return;

        var authorEmail = GetUserEmailForAudit();
        if (authorEmail is null) return;

        adminAction.SetEmergencyMaintenanceState((EmergencyMaintenanceState)argEmergencyMaintenanceState, authorEmail);

        outputProvider.Output("Output Complete.");
    }

    private string? GetUserEmailForAudit()
    {
        var authorEmail = outputProvider.GetString("Please enter your email address for the audit record:");

        if (string.IsNullOrWhiteSpace(authorEmail))
        {
            outputProvider.Output("No email address entered.");
            outputProvider.Output("Exiting without changes...");
            return null;
        }
        
        return authorEmail;
    }
    
    private bool GetUserConfirmationForSettingMaintenanceState(string emergencyMaintenanceVerb)
    {
        outputProvider.Output("!!!!!!!!!!!!!!!!!!!!!!");
        outputProvider.Output(
            $"Please confirm you would like to {emergencyMaintenanceVerb} emergency maintenance mode.");
        var confirmation = outputProvider.Confirm("Would you like to continue? (Y/N)");

        if (!confirmation)
        {
            outputProvider.Output("Exiting without changes...");
        }

        return confirmation;
    }

    private void DisplayMaintenanceStateDetails(string emergencyMaintenanceVerb, EmergencyMaintenanceState liveEmergencyMaintenanceState)
    {
        var isMaintenanceStateEnabled = liveEmergencyMaintenanceState == EmergencyMaintenanceState.Enabled;

        outputProvider.Output("Details:");
        outputProvider.Output($"Request is to {emergencyMaintenanceVerb} emergency maintenance mode.");
        outputProvider.Output($"Portal is currently {(isMaintenanceStateEnabled ? "IN" : "NOT IN")} emergency maintenance mode. Referrals cannot be submitted.");
    }

    private void DisplayMaintenanceStateWarnings()
    {
        outputProvider.Output("!!!!!!!!!!!!!!!!!!!!!!");
        outputProvider.Output("This is a dangerous operation! Read the following before proceeding:");
        outputProvider.Output("This function will enable or disable emergency maintenance mode.");
        outputProvider.Output(
            "In emergency maintenance mode, all requests to this portal will be blocked with a 503 response.");
        outputProvider.Output(
            "This should only be used in accordance with our disaster response plan when usage of the site by members of the public needs to be blocked.");
        outputProvider.Output("If unsure, quit and consult the documentation.");
        outputProvider.Output("!!!!!!!!!!!!!!!!!!!!!!");
    }

    private EmergencyMaintenanceState? ParseEmergencyMaintenanceStateInput(string[] args)
    {
        try
        {
            return Enum.Parse<EmergencyMaintenanceState>(args[0].Trim(), ignoreCase: true);
        }
        catch (Exception e) when (e is ArgumentException or IndexOutOfRangeException)
        {
            var allSubcommands = string.Join("/", Enum.GetValues<EmergencyMaintenanceState>());
            outputProvider.Output(
                $"Please specify whether to enable or disable emergency mode - Usage: SetEmergencyMaintenanceState <{allSubcommands}>");
            outputProvider.Output("Exiting without changes...");
            return null;
        }
    }
}