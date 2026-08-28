using APS.AIMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Persistence;

public class AimsDbContext : DbContext
{
    public AimsDbContext(DbContextOptions<AimsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<AssetLocation> AssetLocations => Set<AssetLocation>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();

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
    }

    private static void ConfigureAsset(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.AssetId)
                .IsUnique();

            entity.HasIndex(x => x.BarcodeValue)
                .IsUnique();

            entity.HasIndex(x => x.SerialNumber);

            entity.Property(x => x.AssetId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.BarcodeValue)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ShortDescription)
                .HasMaxLength(500);

            entity.Property(x => x.SerialNumber)
                .HasMaxLength(200);

            entity.Property(x => x.Manufacturer)
                .HasMaxLength(150);

            entity.Property(x => x.Model)
                .HasMaxLength(150);

            entity.Property(x => x.PartNumber)
                .HasMaxLength(150);

            entity.Property(x => x.LegacyAssetId)
                .HasMaxLength(100);

            entity.Property(x => x.Currency)
                .HasMaxLength(3);

            entity.Property(x => x.AcquisitionCost)
                .HasPrecision(18, 2);

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
        });
    }

    private static void ConfigureAssetCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Subcategories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAssetLocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssetLocation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.BranchId, x.Code })
                .IsUnique();

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasOne(x => x.ParentLocation)
                .WithMany(x => x.ChildLocations)
                .HasForeignKey(x => x.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCompany(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
        });
    }

    private static void ConfigureBranch(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.CompanyId, x.Code })
                .IsUnique();

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Branches)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.BranchId, x.Code })
                .IsUnique();

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasOne(x => x.Branch)
                .WithMany(x => x.Departments)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.EmployeeNumber);

            entity.Property(x => x.EmployeeNumber)
                .HasMaxLength(50);

            entity.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(254);

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}