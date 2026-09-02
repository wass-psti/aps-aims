using APS.AIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Persistence;

public class AimsDbContext(
    DbContextOptions<AimsDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<AssetLocation> AssetLocations => Set<AssetLocation>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AssetTransaction> AssetTransactions => Set<AssetTransaction>();
    public DbSet<AssetCustodyHistory> AssetCustodyHistories => Set<AssetCustodyHistory>();
    public DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();
    public DbSet<AssetCalibration> AssetCalibrations => Set<AssetCalibration>();
    public DbSet<InventoryCampaign> InventoryCampaigns => Set<InventoryCampaign>();
    public DbSet<InventoryCount> InventoryCounts => Set<InventoryCount>();
    public DbSet<AssetIncident> AssetIncidents => Set<AssetIncident>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasSequence<long>("AssetIdSequence")
            .StartsAt(1)
            .IncrementsBy(1);

        ConfigureAsset(modelBuilder);
        ConfigureAssetCategory(modelBuilder);
        ConfigureAssetLocation(modelBuilder);
        ConfigureCompany(modelBuilder);
        ConfigureBranch(modelBuilder);
        ConfigureDepartment(modelBuilder);
        ConfigureEmployee(modelBuilder);
        ConfigureAssetTransaction(modelBuilder);
        ConfigureAssetCustodyHistory(modelBuilder);
        ConfigureAssetMaintenance(modelBuilder);
        ConfigureAssetCalibration(modelBuilder);
        ConfigureInventoryCampaign(modelBuilder);
        ConfigureInventoryCount(modelBuilder);
        ConfigureAssetIncident(modelBuilder);
        ConfigureApplicationUser(modelBuilder);
        ConfigureAuditLog(modelBuilder);
    }

    private static void ConfigureAsset(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Asset>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.AssetId).IsUnique();
        entity.HasIndex(x => x.BarcodeValue).IsUnique();
        entity.HasIndex(x => x.SerialNumber);

        entity.Property(x => x.AssetId).HasMaxLength(50);
        entity.Property(x => x.BarcodeValue).HasMaxLength(200);
        entity.Property(x => x.Name).HasMaxLength(200);
        entity.Property(x => x.ShortDescription).HasMaxLength(500);
        entity.Property(x => x.SerialNumber).HasMaxLength(200);
        entity.Property(x => x.Manufacturer).HasMaxLength(150);
        entity.Property(x => x.Model).HasMaxLength(150);
        entity.Property(x => x.PartNumber).HasMaxLength(150);
        entity.Property(x => x.LegacyAssetId).HasMaxLength(100);
        entity.Property(x => x.Currency).HasMaxLength(3);
        entity.Property(x => x.AcquisitionCost).HasPrecision(18, 2);

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Condition)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CurrentLocation)
            .WithMany()
            .HasForeignKey(x => x.CurrentLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CurrentCustodian)
            .WithMany()
            .HasForeignKey(x => x.CurrentCustodianId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAssetCategory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssetCategory>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.Code).IsUnique();

        entity.Property(x => x.Code).HasMaxLength(50);
        entity.Property(x => x.Name).HasMaxLength(150);
        entity.Property(x => x.Description).HasMaxLength(500);

        entity.HasOne(x => x.ParentCategory)
            .WithMany(x => x.Subcategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAssetLocation(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssetLocation>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.BranchId, x.Code }).IsUnique();

        entity.Property(x => x.Code).HasMaxLength(50);
        entity.Property(x => x.Name).HasMaxLength(150);

        entity.HasOne(x => x.Branch)
            .WithMany(x => x.Locations)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ParentLocation)
            .WithMany(x => x.ChildLocations)
            .HasForeignKey(x => x.ParentLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCompany(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Company>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.Code).IsUnique();
        entity.Property(x => x.Code).HasMaxLength(50);
        entity.Property(x => x.Name).HasMaxLength(150);
    }

    private static void ConfigureBranch(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Branch>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        entity.Property(x => x.Code).HasMaxLength(50);
        entity.Property(x => x.Name).HasMaxLength(150);

        entity.HasOne(x => x.Company)
            .WithMany(x => x.Branches)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Department>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.BranchId, x.Code }).IsUnique();
        entity.Property(x => x.Code).HasMaxLength(50);
        entity.Property(x => x.Name).HasMaxLength(150);

        entity.HasOne(x => x.Branch)
            .WithMany(x => x.Departments)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Employee>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.EmployeeNumber).IsUnique();

        entity.Property(x => x.EmployeeNumber).HasMaxLength(50);
        entity.Property(x => x.FirstName).HasMaxLength(100);
        entity.Property(x => x.LastName).HasMaxLength(100);
        entity.Property(x => x.Email).HasMaxLength(200);

        entity.HasOne(x => x.Department)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAssetTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssetTransaction>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.AssetId, x.OccurredAt });

        entity.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Notes).HasMaxLength(1000);

        entity.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.FromCustodian)
            .WithMany()
            .HasForeignKey(x => x.FromCustodianId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ToCustodian)
            .WithMany()
            .HasForeignKey(x => x.ToCustodianId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.FromLocation)
            .WithMany()
            .HasForeignKey(x => x.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ToLocation)
            .WithMany()
            .HasForeignKey(x => x.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAssetCustodyHistory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssetCustodyHistory>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.AssetId, x.IssuedAt });

        entity.HasIndex(x => x.AssetId)
            .IsUnique()
            .HasFilter("\"ReturnedAt\" IS NULL");

        entity.Property(x => x.IssueNotes).HasMaxLength(1000);
        entity.Property(x => x.ReturnNotes).HasMaxLength(1000);

        entity.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.IssuedFromLocation)
            .WithMany()
            .HasForeignKey(x => x.IssuedFromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ReturnedToLocation)
            .WithMany()
            .HasForeignKey(x => x.ReturnedToLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAssetMaintenance(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssetMaintenance>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.AssetId, x.StartedAt });

        entity.HasIndex(x => x.AssetId)
            .IsUnique()
            .HasFilter("\"CompletedAt\" IS NULL");

        entity.Property(x => x.Description).HasMaxLength(500);
        entity.Property(x => x.ServiceProvider).HasMaxLength(200);
        entity.Property(x => x.StartNotes).HasMaxLength(1000);
        entity.Property(x => x.CompletionNotes).HasMaxLength(1000);
        entity.Property(x => x.Cost).HasPrecision(18, 2);
        entity.Property(x => x.Currency).HasMaxLength(3);

        entity.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAssetCalibration(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssetCalibration>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.AssetId, x.StartedAt });

        entity.HasIndex(x => x.AssetId)
            .IsUnique()
            .HasFilter("\"CompletedAt\" IS NULL");

        entity.Property(x => x.ServiceProvider).HasMaxLength(200);
        entity.Property(x => x.StartNotes).HasMaxLength(1000);
        entity.Property(x => x.CertificateNumber).HasMaxLength(150);
        entity.Property(x => x.CompletionNotes).HasMaxLength(1000);

        entity.Property(x => x.Result)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureInventoryCampaign(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<InventoryCampaign>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.BranchId, x.Status });

        entity.Property(x => x.Name).HasMaxLength(200);
        entity.Property(x => x.Description).HasMaxLength(1000);

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureInventoryCount(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<InventoryCount>();

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => new { x.CampaignId, x.AssetId })
            .IsUnique();

        entity.HasIndex(x => new { x.CampaignId, x.CountedAt });

        entity.Property(x => x.SystemCondition)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.ObservedCondition)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Result)
            .HasConversion<string>()
            .HasMaxLength(80);

        entity.Property(x => x.Notes).HasMaxLength(1000);

        entity.HasOne(x => x.Campaign)
            .WithMany()
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.SystemLocation)
            .WithMany()
            .HasForeignKey(x => x.SystemLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ObservedLocation)
            .WithMany()
            .HasForeignKey(x => x.ObservedLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAssetIncident(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AssetIncident>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.AssetId, x.ReportedAt });
        entity.HasIndex(x => x.Status);

        entity.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        entity.Property(x => x.Description).HasMaxLength(2000);
        entity.Property(x => x.ResolutionNotes).HasMaxLength(2000);

        entity.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }


    private static void ConfigureApplicationUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ApplicationUser>();

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.Email).IsUnique();

        entity.Property(x => x.Email)
            .HasMaxLength(250);

        entity.Property(x => x.PasswordHash)
            .HasMaxLength(500);

        entity.Property(x => x.FirstName)
            .HasMaxLength(100);

        entity.Property(x => x.LastName)
            .HasMaxLength(100);

        entity.Property(x => x.Role)
            .HasMaxLength(50);
    }


    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuditLog>();

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.OccurredAt);
        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => new { x.Resource, x.OccurredAt });

        entity.Property(x => x.UserEmail)
            .HasMaxLength(250);

        entity.Property(x => x.UserDisplayName)
            .HasMaxLength(200);

        entity.Property(x => x.UserRole)
            .HasMaxLength(50);

        entity.Property(x => x.Action)
            .HasMaxLength(150);

        entity.Property(x => x.Resource)
            .HasMaxLength(120);

        entity.Property(x => x.ResourceId)
            .HasMaxLength(120);

        entity.Property(x => x.HttpMethod)
            .HasMaxLength(16);

        entity.Property(x => x.Path)
            .HasMaxLength(1000);

        entity.Property(x => x.IpAddress)
            .HasMaxLength(80);

        entity.Property(x => x.UserAgent)
            .HasMaxLength(500);
    }

}
