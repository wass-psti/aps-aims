using APS.AIMS.Application.AssetCategories;
using APS.AIMS.Application.AssetLocations;
using APS.AIMS.Application.Branches;
using APS.AIMS.Application.Companies;
using APS.AIMS.Application.Departments;
using APS.AIMS.Infrastructure.Persistence;
using APS.AIMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APS.AIMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("AimsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'AimsDatabase' was not found.");

        services.AddDbContext<AimsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAssetLocationService, AssetLocationService>();
        services.AddScoped<IAssetCategoryService, AssetCategoryService>();
        
        return services;
    }
}