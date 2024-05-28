using HerPortal.BusinessLogic;
using HerPortal.BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace HerPortal.Data;

public class DataAccessProvider : IDataAccessProvider
{
    private readonly HerDbContext context;

    public DataAccessProvider(HerDbContext context)
    {
        this.context = context;
    }

    public async Task<User> GetUserByEmailAsync(string emailAddress)
    {
        var users = await context.Users
            .Include(u => u.LocalAuthorities)
            .Include(u => u.Consortia)
            // In order to compare email addresses case-insensitively, we bring the whole table into memory here
            //   to perform the comparison in C#, since Entity Framework doesn't allow for the StringComparison
            //   overload. However, since we don't expect this table to be monstrously huge this is acceptable
            //   in order to easily allow case-insensitive email addresses.
            .ToListAsync();
        return users
            .Single(u => string.Equals
                (
                    u.EmailAddress,
                    emailAddress,
                    StringComparison.CurrentCultureIgnoreCase
                ));
    }

    public async Task MarkUserAsHavingLoggedInAsync(int userId)
    {
        var user = await context.Users
            .SingleAsync(u => u.Id == userId);

        user.HasLoggedIn = true;
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllActiveUsersAsync()
    {
        return await context.Users
            .Where(u => u.HasLoggedIn)
            .Include(u => u.LocalAuthorities)
            .Include(u => u.Consortia)
            .ToListAsync();
    }

    public async Task<List<CsvFileDownload>> GetCsvFileDownloadDataForUserAsync(int userId)
    {
        return await context.CsvFileDownloads.Where(cfd => cfd.UserId == userId).ToListAsync();
    }

    public async Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId)
    {
        User user;
        try
        {
            user = await context.Users.SingleAsync(u => u.Id == userId);
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentOutOfRangeException($"No user found with ID {userId}", ex);
        }
        
        CsvFileDownload download;
        try
        {
            download = await context.CsvFileDownloads
                .SingleAsync(cfd =>
                    cfd.CustodianCode == custodianCode &&
                    cfd.Year == year &&
                    cfd.Month == month &&
                    cfd.UserId == userId
                );
        }
        catch (InvalidOperationException)
        {
            download = new CsvFileDownload
            {
                CustodianCode = custodianCode,
                Year = year,
                Month = month,
                UserId = userId,
            };
            await context.CsvFileDownloads.AddAsync(download);
        }

        download.LastDownloaded = DateTime.Now;

        var auditDownload = new AuditDownload
        {
            CustodianCode = custodianCode,
            Year = year,
            Month = month,
            UserEmail = user.EmailAddress,
            Timestamp = DateTime.Now,
        };
        await context.AuditDownloads.AddAsync(auditDownload);
        
        await context.SaveChangesAsync();
    }

    public List<string> GetConsortiumCodesForUser(User user)
    {
        var userLocalAuthorities = user.LocalAuthorities.Select(la => la.CustodianCode);
        var userConsortia = user.Consortia.Select(consortium => consortium.ConsortiumCode);

        // user is a consortium manager if they are a manager of all LAs in that consortium
        return ConsortiumData.ConsortiumCustodianCodesIdsByConsortiumCode
            .Where(pair => pair.Value.All(consortiumLa => userLocalAuthorities.Contains(consortiumLa)))
            .Select(pair => pair.Key)
            //Include all explicitly listed consortium codes
            .Union(userConsortia)
            .ToList();
    }
}
