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

        entity.HasIndex(x => x.EmployeeNumber)
            .IsUnique();

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

        entity.Property(x => x.Notes)
            .HasMaxLength(1000);

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

        entity.Property(x => x.IssueNotes)
            .HasMaxLength(1000);

        entity.Property(x => x.ReturnNotes)
            .HasMaxLength(1000);

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
}
