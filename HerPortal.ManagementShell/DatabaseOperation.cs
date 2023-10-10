using HerPortal.BusinessLogic.Models;
using HerPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace HerPortal.ManagementShell; 

public class DatabaseOperation : IDatabaseOperation
{
    private readonly HerDbContext dbContext;
    private readonly OutputProvider outputProvider;

    public DatabaseOperation(HerDbContext dbContext, OutputProvider outputProvider)
    {
        this.dbContext = dbContext;
        this.outputProvider = outputProvider;
    }

    public List<User> GetUsersWithLocalAuthorities()
    {
        return dbContext.Users
            .Include(user => user.LocalAuthorities)
            .ToList();
    }

    public void RemoveUserOrLogError(User? user)
    {
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            switch (user)
            {
                // removing a user also deletes all associated rows in the LocalAuthorityUser table
                case null:
                    outputProvider.Output("User not found");
                    break;
                default:
                {
                    {
                        dbContext.Users.Remove(user);
                        dbContext.SaveChanges();
                        var deletionConfirmation = outputProvider.Confirm(
                            $"Attention! This will delete user {user.EmailAddress} and all associated rows from the database. Are you sure you want to commit this transaction? (y/n)");

                        if (deletionConfirmation)
                        {
                            dbContextTransaction.Commit();
                            outputProvider.Output($"Operation successful. User {user.EmailAddress} was deleted");
                        }
                        else
                        {
                            dbContextTransaction.Rollback();
                            outputProvider.Output("Rollback complete");
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception e)
        {
            outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
            dbContextTransaction.Rollback();
        }
    }

    public void CreateUserOrLogError(string userEmailAddress, string[]? custodianCodes)
    {
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            var newLaUser = new User
            {
                EmailAddress = userEmailAddress,
                HasLoggedIn = false,
                LocalAuthorities = custodianCodes!
                    .Select(code => dbContext.LocalAuthorities
                        .Single(la => la.CustodianCode == code))
                    .ToList()
            };
            dbContext.Add(newLaUser);
            dbContext.SaveChanges();
            outputProvider.Output("Operation successful");
            dbContextTransaction.Commit();
        }
        catch (Exception e)
        {
            outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
            dbContextTransaction.Rollback();
        }
    }

    public void RemoveLasFromUser(string[]? custodianCodes, User? user)
    {
        var lasToRemove = user?.LocalAuthorities.Where(la => custodianCodes!.Contains(la.CustodianCode)).ToList();

        if (custodianCodes != null && lasToRemove != null && lasToRemove.Count < custodianCodes.Length)
        {
            outputProvider.Output("Number of LAs to remove is less than the number of custodian codes submitted. Please check custodian codes");
        }
        
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            if (lasToRemove != null)
                foreach (var la in lasToRemove)
                {
                    user?.LocalAuthorities.Remove(la);
                }

            dbContext.SaveChanges();
            outputProvider.Output("Operation successful");
            dbContextTransaction.Commit();
        }
        catch (Exception e)
        {
            outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
            dbContextTransaction.Rollback();
        }
    }

    public void AddLasToUser(string[]? custodianCodes, User? user)
    {
        if (custodianCodes == null) return;
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            foreach (var code in custodianCodes)
            {
                try
                {
                    var la = dbContext.LocalAuthorities.SingleOrDefault(la => la.CustodianCode == code);
                    user?.LocalAuthorities.Add(la);
                }
                catch (Exception e)
                {
                    outputProvider.Output(e.Message);
                    throw;
                }
            }

            dbContext.SaveChanges();
            outputProvider.Output("Operation successful");
            dbContextTransaction.Commit();
        }
        catch (Exception e)
        {
            outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
            dbContextTransaction.Rollback();
        }
    }
}