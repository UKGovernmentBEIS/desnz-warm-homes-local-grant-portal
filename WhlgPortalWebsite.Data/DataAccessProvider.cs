﻿using Microsoft.EntityFrameworkCore;
using WhlgPortalWebsite.BusinessLogic;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;

namespace WhlgPortalWebsite.Data;

public class DataAccessProvider : IDataAccessProvider
{
    private readonly WhlgDbContext context;

    public DataAccessProvider(WhlgDbContext context)
    {
        this.context = context;
    }

    public async Task<User> GetUserByEmailAsync(string emailAddress)
    {
        var users = await context
            .Users
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

    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await context
            .Users
            .Where(u => u.Id == userId)
            .Include(u => u.LocalAuthorities)
            .Include(u => u.Consortia)
            .SingleAsync();
    }

    public async Task MarkUserAsHavingLoggedInAsync(int userId)
    {
        var user = await context.Users
            .SingleAsync(u => u.Id == userId);

        user.HasLoggedIn = true;
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllActiveDeliveryPartnersAsync()
    {
        return await context.Users
            .Where(u => u.HasLoggedIn && u.Role == UserRole.DeliveryPartner)
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
                UserId = userId
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
            Timestamp = DateTime.Now
        };
        await context.AuditDownloads.AddAsync(auditDownload);

        await context.SaveChangesAsync();
    }

    public async Task<IList<User>> GetAllDeliveryPartnersAsync()
    {
        return await context
            .Users
            .Where(u => u.Role == UserRole.DeliveryPartner)
            .Include(u => u.LocalAuthorities)
            .Include(u => u.Consortia)
            .ToListAsync();
    }

    public async Task<IList<User>> GetAllDeliveryPartnersWhereEmailContainsAsync(string partialEmailAddress)
    {
        var users = await context
            .Users
            .Where(u => u.Role == UserRole.DeliveryPartner)
            .Include(u => u.LocalAuthorities)
            .Include(u => u.Consortia)
            .ToListAsync();
        // similar to GetUserByEmailAsync, we must pull in the table to compare case insensitively
        return users
            .Where(u => u.EmailAddress.Contains
            (
                partialEmailAddress,
                StringComparison.CurrentCultureIgnoreCase
            ))
            .ToList();
    }

    public async Task<User> CreateDeliveryPartnerAsync(string userEmailAddress)
    {
        var newUser = new User
        {
            EmailAddress = userEmailAddress,
            Role = UserRole.DeliveryPartner,
            HasLoggedIn = false,
            LocalAuthorities = new List<LocalAuthority>(),
            Consortia = new List<Consortium>()
        };

        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();

        return newUser;
    }

    public async Task<bool> IsEmailAddressInUseAsync(string emailAddress)
    {
        var users = await context
            .Users
            .ToListAsync();

        return users.Any(u => string.Equals
        (
            u.EmailAddress,
            emailAddress,
            StringComparison.CurrentCultureIgnoreCase
        ));
    }

    public async Task<IEnumerable<LocalAuthority>> GetAllLasAsync()
    {
        return await context.LocalAuthorities.ToListAsync();
    }

    public async Task<IEnumerable<Consortium>> GetAllConsortiaAsync()
    {
        return await context.Consortia.ToListAsync();
    }

    public async Task AddLaToDeliveryPartnerAsync(User user, LocalAuthority localAuthority)
    {
        user.LocalAuthorities.Add(localAuthority);
        await context.SaveChangesAsync();
    }

    public async Task AddConsortiumToDeliveryPartnerAsync(User user, Consortium consortium)
    {
        user.Consortia.Add(consortium);
        await context.SaveChangesAsync();
    }

    public async Task<LocalAuthority> GetLocalAuthorityByCustodianCodeAsync(string custodianCode)
    {
        return await context.LocalAuthorities
            .SingleAsync(la => la.CustodianCode == custodianCode);
    }

    public async Task<Consortium> GetConsortiumByConsortiumCodeAsync(string consortiumCode)
    {
        return await context.Consortia
            .SingleAsync(c => c.ConsortiumCode == consortiumCode);
    }
}