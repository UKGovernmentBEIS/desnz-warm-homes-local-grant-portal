using HerPortal.BusinessLogic.Models;

namespace HerPortal.ManagementShell;

public class AdminAction
{
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

    public bool ConfirmCustodianCodes(string[] codes, string userEmailAddress)
    {
        outputProvider.Output("ATTENTION! POTENTIAL DATA BREACH! Make sure below user should have the permissions you are about to grant");
        outputProvider.Output(
            $"You are changing permissions for user {userEmailAddress} for the following local authorities: ");

        foreach (var code in codes)

        {
            var localAuthority = custodianCodeToLaDict[code];
            outputProvider.Output($"{code}: {localAuthority}");
        }

        var hasUserConfirmed = outputProvider.Confirm("Please confirm (y/n)");
        if (!hasUserConfirmed)
        {
            outputProvider.Output("Process cancelled, no changes were made to the database");
        }

        return hasUserConfirmed;
    }

    public void DisplayUserStatus(Enum status)
    {
        switch (status)
        {
            case Program.UserStatus.New:
                outputProvider.Output("User not found in database. A new user will be created");
                break;
            case Program.UserStatus.Active:
                outputProvider.Output("User found in database. LAs will be added to their account");
                break;
        }
    }

    public Enum GetUserStatus(User? userOrNull)
    {
        return userOrNull == null ? Program.UserStatus.New : Program.UserStatus.Active;
    }

    public void TryCreateUser(string userEmailAddress, string[]? custodianCodes)
    {
        dbOperation.CreateUserOrLogError(userEmailAddress, custodianCodes);
    }

    public void TryRemoveUser(User? user)
    {
        dbOperation.RemoveUserOrLogError(user);
    }
    public void AddLas(string[]? custodianCodes, User? user)
    {
        dbOperation.AddLasToUser(custodianCodes, user);
    }

    public void RemoveLas(string[]? custodianCodes, User? user)
    {
        dbOperation.RemoveLasFromUser(custodianCodes, user);
    }

    public void Output(string outputString)
    {
        outputProvider.Output(outputString);
    }
}