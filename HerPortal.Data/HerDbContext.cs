using HerPortal.BusinessLogic.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HerPortal.Data;

public class HerDbContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<TrackedCsvFile> TrackedCsvFiles { get; set; }
    public DbSet<LocalAuthority> LocalAuthorities { get; set; }
    public DbSet<User> Users { get; set; }

    public HerDbContext(DbContextOptions<HerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<User>()
            .HasIndex(u => u.EmailAddress)
            .IsUnique();

        modelBuilder
            .Entity<LocalAuthority>()
            .HasIndex(la => la.CustodianCode)
            .IsUnique();
        
        modelBuilder
            .Entity<CsvFileDownload>()
            .HasKey(cf => new
            {
                cf.CsvFileId,
                cf.UserId,
                cf.Timestamp,
            });
        
        modelBuilder
            .Entity<TrackedCsvFile>()
            .HasKey(tcf => new
            {
                tcf.Id
            });
        
        AddAllRowVersioning(modelBuilder);
    }

    private void AddAllRowVersioning(ModelBuilder modelBuilder)
    {
        AddRowVersionColumn(modelBuilder.Entity<CsvFileDownload>());
        AddRowVersionColumn(modelBuilder.Entity<LocalAuthority>());
        AddRowVersionColumn(modelBuilder.Entity<User>());
    }

    private void AddRowVersionColumn<T>(EntityTypeBuilder<T> builder) where T : class
    {
        // This is a PostgreSQL specific implementation of row versioning
        builder.UseXminAsConcurrencyToken();
    }
}
