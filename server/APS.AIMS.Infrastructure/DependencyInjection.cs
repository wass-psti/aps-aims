using APS.AIMS.Application.Auditing;
using APS.AIMS.Infrastructure.Security;
using APS.AIMS.Application.Users;
using APS.AIMS.Application.Authentication;
using APS.AIMS.Application.AssetCategories;
using APS.AIMS.Application.AssetLocations;
using APS.AIMS.Application.Assets;
using APS.AIMS.Application.Branches;
using APS.AIMS.Application.Calibration;
using APS.AIMS.Application.Companies;
using APS.AIMS.Application.Custody;
using APS.AIMS.Application.Departments;
using APS.AIMS.Application.Employees;
using APS.AIMS.Application.Maintenance;
using APS.AIMS.Application.Reports;
using APS.AIMS.Application.Inventory;
using APS.AIMS.Application.Incidents;
using APS.AIMS.Application.Transactions;
using APS.AIMS.Infrastructure.Persistence;
using APS.AIMS.Infrastructure.Configuration;
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
        var configuredConnectionString =
            configuration.GetConnectionString("AimsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'AimsDatabase' was not found.");

        var connectionString =
            PostgresConnectionStringNormalizer.Normalize(
                configuredConnectionString);

        services.AddDbContext<AimsDbContext>(
            options =>
                options.UseNpgsql(
                    connectionString,
                    npgsql =>
                    {
                        npgsql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay:
                                TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);

                        npgsql.CommandTimeout(30);
                    }));

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAssetLocationService, AssetLocationService>();
        services.AddScoped<IAssetCategoryService, AssetCategoryService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAssetCustodyService, AssetCustodyService>();
        services.AddScoped<IAssetTransactionService, AssetTransactionService>();
        services.AddScoped<IAssetMaintenanceService, AssetMaintenanceService>();
        services.AddScoped<IAssetCalibrationService, AssetCalibrationService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IAssetIncidentService, AssetIncidentService>();
        services.AddScoped<IAssetReportService, AssetReportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();

        return services;
    }
}
