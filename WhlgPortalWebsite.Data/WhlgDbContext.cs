using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.Data;

public class WhlgDbContext(DbContextOptions<WhlgDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<AuditDownload> AuditDownloads { get; set; }
    public DbSet<CsvFileDownload> CsvFileDownloads { get; set; }
    public DbSet<LocalAuthority> LocalAuthorities { get; set; }
    public DbSet<Consortium> Consortia { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<EmergencyMaintenanceHistory> EmergencyMaintenanceHistories { get; set; }

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
            .Entity<AuditDownload>()
            .Property<int>("Id")
            .HasColumnType("integer")
            .ValueGeneratedOnAdd();

        modelBuilder
            .Entity<AuditDownload>()
            .HasKey("Id");

        modelBuilder
            .Entity<AuditDownload>()
            .Property(cf => cf.Timestamp)
            .HasColumnType("timestamp without time zone");

        modelBuilder
            .Entity<AuditDownload>()
            .Property(cf => cf.UserEmail)
            .IsRequired();

        modelBuilder
            .Entity<AuditDownload>()
            .Property(cf => cf.CustodianCode)
            .IsRequired();

        modelBuilder
            .Entity<CsvFileDownload>()
            .HasKey(cf => new
            {
                cf.CustodianCode,
                cf.Year,
                cf.Month,
                cf.UserId,
            });

        modelBuilder
            .Entity<CsvFileDownload>()
            .Property(cf => cf.LastDownloaded)
            .HasColumnType("timestamp without time zone");

        SetupEmergencyMaintenanceHistory(modelBuilder);

        AddAllRowVersioning(modelBuilder);
    }

    private void SetupEmergencyMaintenanceHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmergencyMaintenanceHistory>()
            .Property(emh => emh.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<EmergencyMaintenanceHistory>()
            .HasKey("Id");
        modelBuilder.Entity<EmergencyMaintenanceHistory>()
            .Property(emh => emh.ChangeDate)
            .HasColumnType("timestamp with time zone");
        modelBuilder.Entity<EmergencyMaintenanceHistory>()
            .HasIndex(emh => emh.ChangeDate);

        AddRowVersionColumn(modelBuilder.Entity<EmergencyMaintenanceHistory>());
    }


    private void AddAllRowVersioning(ModelBuilder modelBuilder)
    {
        AddRowVersionColumn(modelBuilder.Entity<CsvFileDownload>());
        AddRowVersionColumn(modelBuilder.Entity<LocalAuthority>());
        AddRowVersionColumn(modelBuilder.Entity<User>());
    }

    private void AddRowVersionColumn<T>(EntityTypeBuilder<T> builder) where T : class, IEntityWithRowVersioning
    {
        // Instruct EF to use Postgres specific implementation of row versioning
        // See https://www.npgsql.org/efcore/modeling/concurrency.html
        builder
            .Property(b => b.Version)
            .IsRowVersion();
    }
}