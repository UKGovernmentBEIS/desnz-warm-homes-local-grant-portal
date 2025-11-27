using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;
using WhlgPortalWebsite.Data;

namespace WhlgPortalWebsite.ManagementShell;

[ExcludeFromCodeCoverage]
public class DatabaseOperation(WhlgDbContext dbContext, OutputProvider outputProvider) : IDatabaseOperation
{
    public List<User> GetUsersIncludingLocalAuthoritiesAndConsortia()
    {
        return dbContext.Users
            .Include(user => user.LocalAuthorities)
            .Include(user => user.Consortia)
            .ToList();
    }

    public List<LocalAuthority> GetLas(IReadOnlyCollection<string> custodianCodes)
    {
        var missingCustodianCode =
            custodianCodes.SingleOrDefault(code => !dbContext.LocalAuthorities.Any(la => la.CustodianCode == code),
                null);
        if (missingCustodianCode != null)
            throw new CouldNotFindAuthorityException("Could not find Custodian Code in database.",
                new List<string> { missingCustodianCode });

        return custodianCodes
            .Select(code => dbContext.LocalAuthorities
                .Single(la => la.CustodianCode == code))
            .ToList();
    }

    public List<Consortium> GetConsortia(IReadOnlyCollection<string> consortiumCodes)
    {
        var missingConsortiumCode =
            consortiumCodes.SingleOrDefault(code => !dbContext.Consortia.Any(la => la.ConsortiumCode == code), null);
        if (missingConsortiumCode != null)
            throw new CouldNotFindAuthorityException("Could not find Consortium Code in database.",
                new List<string> { missingConsortiumCode });

        return consortiumCodes
            .Select(code => dbContext.Consortia
                .Single(la => la.ConsortiumCode == code))
            .ToList();
    }

    public void RemoveUserOrLogError(User user)
    {
        PerformTransaction(() =>
        {
            // removing a user also deletes all associated rows in the LocalAuthorityUser table
            dbContext.Users.Remove(user);
        });
    }

    public void CreateUserOrLogError(string userEmailAddress, UserRole userRole, List<LocalAuthority> localAuthorities,
        List<Consortium> consortia)
    {
        PerformTransaction(() =>
        {
            var newUser = new User
            {
                EmailAddress = userEmailAddress,
                Role = userRole,
                HasLoggedIn = false,
                LocalAuthorities = localAuthorities,
                Consortia = consortia
            };
            dbContext.Add(newUser);
        });
    }

    public void RemoveLasFromUser(User user, List<LocalAuthority> lasToRemove)
    {
        PerformTransaction(() =>
        {
            foreach (var la in lasToRemove) user?.LocalAuthorities.Remove(la);
        });
    }

    public void AddConsortiaAndRemoveLasFromUser(User user, List<Consortium> consortia,
        List<LocalAuthority> localAuthorities)
    {
        PerformTransaction(() =>
        {
            foreach (var consortium in consortia) user?.Consortia.Add(consortium);
            foreach (var localAuthority in localAuthorities) user?.LocalAuthorities.Remove(localAuthority);
        });
    }

    public void RemoveConsortiaFromUser(User user, List<Consortium> consortia)
    {
        PerformTransaction(() =>
        {
            foreach (var consortium in consortia) user?.Consortia.Remove(consortium);
        });
    }

    public void AddLasToUser(User user, List<LocalAuthority> localAuthorities)
    {
        PerformTransaction(() =>
        {
            foreach (var la in localAuthorities)
                try
                {
                    user?.LocalAuthorities.Add(la);
                }
                catch (Exception e)
                {
                    outputProvider.Output(e.Message);
                    throw;
                }
        });
    }

    public void AddConsortiaToUser(User user, List<Consortium> consortia)
    {
        PerformTransaction(() =>
        {
            foreach (var consortium in consortia)
                try
                {
                    user?.Consortia.Add(consortium);
                }
                catch (Exception e)
                {
                    outputProvider.Output(e.Message);
                    throw;
                }
        });
    }

    public IEnumerable<LocalAuthority> GetAllLas()
    {
        return dbContext.LocalAuthorities;
    }

    public IEnumerable<Consortium> GetAllConsortia()
    {
        return dbContext.Consortia;
    }

    public void CreateLasAndConsortia(IEnumerable<string> custodianCodes, IEnumerable<string> consortiumCodes)
    {
        var las = custodianCodes.Select(custodianCode => new LocalAuthority
        {
            CustodianCode = custodianCode,
            Users = []
        }).ToList();
        var consortia = consortiumCodes.Select(consortiumCode => new Consortium
        {
            ConsortiumCode = consortiumCode,
            Users = []
        }).ToList();

        PerformTransaction(() =>
        {
            dbContext.LocalAuthorities.AddRange(las);
            dbContext.Consortia.AddRange(consortia);
        });
    }
    
    public void AddEmergencyMaintenanceHistory(EmergencyMaintenanceHistory history)
    {
        PerformTransaction(() =>
        {
            dbContext.EmergencyMaintenanceHistories.Add(history);
        });
    }

    public EmergencyMaintenanceHistory? GetLatestEmergencyMaintenanceHistory()
    {
        return dbContext.EmergencyMaintenanceHistories.OrderByDescending(emh => emh.ChangeDate).FirstOrDefault();
    }

    private void PerformTransaction(Action transaction)
    {
        using var dbContextTransaction = dbContext.Database.BeginTransaction();
        try
        {
            transaction();

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