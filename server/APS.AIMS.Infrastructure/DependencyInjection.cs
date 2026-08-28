using APS.AIMS.Infrastructure.Persistence;
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

        return services;
    }
}