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
        var users = await context.Users.ToListAsync();
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
