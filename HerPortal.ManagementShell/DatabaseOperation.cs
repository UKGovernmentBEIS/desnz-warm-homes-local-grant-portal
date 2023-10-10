using System.Diagnostics.CodeAnalysis;
using HerPortal.BusinessLogic.Models;
using HerPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace HerPortal.ManagementShell;

[ExcludeFromCodeCoverage]
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

    public List<LocalAuthority> GetLas(string[] custodianCodes)
    {
        return custodianCodes
            .Select(code => dbContext.LocalAuthorities
                .Single(la => la.CustodianCode == code))
            .ToList();
    }

    public void RemoveUserOrLogError(User user)
    {
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            // removing a user also deletes all associated rows in the LocalAuthorityUser table
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
            dbContextTransaction.Commit();
            outputProvider.Output($"Operation successful. User {user.EmailAddress} was deleted");
        }
        catch (Exception e)
        {
            dbContextTransaction.Rollback();
            outputProvider.Output($"Rollback following error in transaction: {e.InnerException?.Message}");
        }
    }

    public void CreateUserOrLogError(string userEmailAddress, List<LocalAuthority> localAuthorities)
    {
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            var newLaUser = new User
            {
                EmailAddress = userEmailAddress,
                HasLoggedIn = false,
                LocalAuthorities = localAuthorities
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

    public void RemoveLasFromUser(User user, List<LocalAuthority> lasToRemove)
    {
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
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

    public void AddLasToUser(User user, List<LocalAuthority> localAuthorities)
    {
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            foreach (var la in localAuthorities)
            {
                try
                {
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