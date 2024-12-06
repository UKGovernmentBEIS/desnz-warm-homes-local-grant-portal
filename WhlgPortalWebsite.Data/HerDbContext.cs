using HerPortal.BusinessLogic.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HerPortal.Data;

public class HerDbContext : DbContext, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<AuditDownload> AuditDownloads { get; set; }
    public DbSet<CsvFileDownload> CsvFileDownloads { get; set; }
    public DbSet<LocalAuthority> LocalAuthorities { get; set; }
    public DbSet<Consortium> Consortia { get; set; }
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
