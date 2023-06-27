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
            .ToListAsync();
    }

    public async Task<List<TrackedCsvFile>> GetCsvFilesDownloadedByUserAsync(int userId)
    {
        return await context.TrackedCsvFiles
            .Where(tcf => tcf.Downloads.Any(d => d.UserId == userId))
            .Include(tcf => tcf.Downloads)
            .ToListAsync();
    }

    public async Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId)
    {
        if (!await context.Users.AnyAsync(u => u.Id == userId))
        {
            throw new ArgumentOutOfRangeException($"No user found with ID {userId}");
        }
        
        TrackedCsvFile file;
        try
        {
            file = await context.TrackedCsvFiles
                .Include(tcf => tcf.Downloads)
                .SingleAsync(cfd =>
                    cfd.CustodianCode == custodianCode &&
                    cfd.Year == year &&
                    cfd.Month == month
                );
        }
        catch (InvalidOperationException)
        {
            file = new TrackedCsvFile
            {
                CustodianCode = custodianCode,
                Year = year,
                Month = month,
                Downloads = new List<CsvFileDownload>(),
            };
            await context.TrackedCsvFiles.AddAsync(file);
        }

        file.Downloads.Add(new CsvFileDownload
        {
            Timestamp = DateTime.Now,
            UserId = userId,
        });
        
        await context.SaveChangesAsync();
    }
}
