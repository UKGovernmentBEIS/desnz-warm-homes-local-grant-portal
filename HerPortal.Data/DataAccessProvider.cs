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
}