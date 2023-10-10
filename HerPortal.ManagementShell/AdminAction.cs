using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public class AdminAction
{
    public enum UserStatus
    {
        New,
        Active
    }

    private readonly IDatabaseOperation dbOperation;
    private readonly IOutputProvider outputProvider;
    private readonly Dictionary<string, string> custodianCodeToLaDict = LocalAuthorityData.LocalAuthorityNamesByCustodianCode;

    public AdminAction(IDatabaseOperation dbOperation, IOutputProvider outputProvider)
    {
        this.dbOperation = dbOperation;
        this.outputProvider = outputProvider;
    }

    public User? GetUser(string emailAddress)
    {
        var portalUsers = dbOperation.GetUsersWithLocalAuthorities();
        return
            portalUsers.SingleOrDefault(user => string.Equals
            (
                user.EmailAddress,
                emailAddress,
                StringComparison.CurrentCultureIgnoreCase
            ));
    }

    private bool ConfirmCustodianCodes(string userEmailAddress, string[] codes)
    {
        outputProvider.Output(
            $"You are changing permissions for user {userEmailAddress} for the following local authorities:");

        if (codes.Length < 1)
        {
            outputProvider.Output("(No LAs specified)");
        }

        foreach (var code in codes)

        {
            try
            {
                var localAuthority = custodianCodeToLaDict[code];
                outputProvider.Output($"{code}: {localAuthority}");
            }
            catch (Exception e)
            {
                outputProvider.Output($"{e.Message} Process terminated");
                return false;
            }
        }

        var hasUserConfirmed = outputProvider.Confirm("Please confirm (y/n)");
        if (!hasUserConfirmed)
        {
            outputProvider.Output("Process cancelled, no changes were made to the database");
        }

        return hasUserConfirmed;
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

    public Enum GetUserStatus(User? userOrNull)
    {
        return userOrNull == null ? UserStatus.New : UserStatus.Active;
    }

    private void TryCreateUser(string userEmailAddress, string[]? custodianCodes)
    {
        var lasToAdd = dbOperation.GetLas(custodianCodes ?? Array.Empty<string>());
        dbOperation.CreateUserOrLogError(userEmailAddress, lasToAdd);
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
        if (!deletionConfirmation)
        {
            return;
        }

        dbOperation.RemoveUserOrLogError(user);
    }
    private void AddLas(User? user, string[]? custodianCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (custodianCodes == null || custodianCodes.Length < 1)
        {
            outputProvider.Output("Please specify custodian codes to add to user");
            return;
        }

        var lasToAdd = dbOperation.GetLas(custodianCodes);
        dbOperation.AddLasToUser(user, lasToAdd);
    }

    public void RemoveLas(User? user, string[]? custodianCodes)
    {
        if (user == null)
        {
            outputProvider.Output("User not found");
            return;
        }

        if (custodianCodes == null || custodianCodes.Length < 1)
        {
            outputProvider.Output("Please specify custodian codes to remove from user");
            return;
        }

        var userConfirmation = ConfirmCustodianCodes(user.EmailAddress, custodianCodes);
        if (!userConfirmation)
        {
            return;
        }

        var lasToRemove = user.LocalAuthorities.Where(la => custodianCodes.Contains(la.CustodianCode)).ToList();
        var missingCodes = custodianCodes.Where(code => !lasToRemove.Any(la => la.CustodianCode.Equals(code))).ToList();
        if (missingCodes.Count > 0)
        {
            outputProvider.Output($"Could not find LAs attached to {user.EmailAddress} for the following codes: {string.Join(", ", missingCodes)}. Please check your inputs and try again.");
            return;
        }

        dbOperation.RemoveLasFromUser(user, lasToRemove);
    }

    public void CreateOrUpdateUser(string? userEmailAddress, string[] custodianCodes)
    {
        if (userEmailAddress == null)
        {
            outputProvider.Output("Please specify user E-mail address to create or update");
            return;
        }

        var user = GetUser(userEmailAddress);
        var userStatus = GetUserStatus(user);
        DisplayUserStatus(userStatus);
        outputProvider.Output("");

        outputProvider.Output("!!! ATTENTION! READ CAREFULLY OR RISK A DATA BREACH !!!");
        outputProvider.Output("");
        outputProvider.Output("You are about to grant a user permission to read PERSONALLY IDENTIFIABLE INFORMATION submitted to LAs.");
        outputProvider.Output("Take a moment to double check the following list and only continue if you are certain this user should have access to these LAs.");
        outputProvider.Output("NB: in particular, you should only do this if for LAs that have signed their DSA contracts!");
        outputProvider.Output("");

        var confirmation = ConfirmCustodianCodes(userEmailAddress, custodianCodes);

        if (confirmation)
        {
            if (userStatus.Equals(UserStatus.Active))
            {
                AddLas(user, custodianCodes);
            }
            else if (userStatus.Equals(UserStatus.New))
            {
                TryCreateUser(userEmailAddress, custodianCodes);
            }
        }
    }
}