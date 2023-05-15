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

    public async Task<bool> DoesCsvFileDownloadDataExistAsync(string custodianCode, int year, int month)
    {
        return await context.CsvFileDownloadData.AnyAsync(cf =>
            cf.CustodianCode == custodianCode && cf.Year == year && cf.Month == month);
    }

    public async Task<CsvFileDownloadData> GetCsvFileDownloadDataAsync(string custodianCode, int year, int month)
    {
        CsvFileDownloadData result;
        try
        {
            result = await context.CsvFileDownloadData
                .Include(cf => cf.Downloads)
                .ThenInclude(d => d.User)
                .SingleAsync(cf => cf.CustodianCode == custodianCode && cf.Year == year && cf.Month == month);
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentOutOfRangeException
            (
                $"No data found for CSV file with custodian code {custodianCode}, year {year}, month {month}",
                ex
            );
        }
        return result;
    }

    public async Task BeginTrackingCsvFileDownloadsAsync(string custodianCode, int year, int month)
    {
        if (await DoesCsvFileDownloadDataExistAsync(custodianCode, year, month))
        {
            throw new InvalidOperationException
            (
                $"Cannot begin tracking CSV file (custodian code {custodianCode}, year {year}, month {month}) as it is already being tracked"
            );
        }

        var newFileData = new CsvFileDownloadData
        {
            CustodianCode = custodianCode,
            Year = year,
            Month = month,
            Downloads = new List<CsvFileDownload>(),
        };

        await context.CsvFileDownloadData.AddAsync(newFileData);
        await context.SaveChangesAsync();
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

        var fileData = await GetCsvFileDownloadDataAsync(custodianCode, year, month);
        var newDownload = new CsvFileDownload
        {
            DateTime = DateTime.Now,
            User = user,
        };

        fileData.Downloads.Add(newDownload);
        await context.SaveChangesAsync();
    }
}
