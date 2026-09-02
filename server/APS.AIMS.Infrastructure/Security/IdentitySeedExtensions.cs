using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Security;
using APS.AIMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APS.AIMS.Infrastructure.Security;

public static class IdentitySeedExtensions
{
    public static async Task EnsureAimsIdentitySeededAsync(
        this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AimsDbContext>();

        if (await dbContext.ApplicationUsers.AnyAsync())
        {
            return;
        }

        var configuration =
            scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

        var email =
            configuration["BootstrapAdmin:Email"];
        var password =
            configuration["BootstrapAdmin:Password"];
        var firstName =
            configuration["BootstrapAdmin:FirstName"]
            ?? "APS";
        var lastName =
            configuration["BootstrapAdmin:LastName"]
            ?? "Administrator";

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "No application users exist. Configure BootstrapAdmin:Email and BootstrapAdmin:Password in User Secrets before starting APS AIMS.");
        }

        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        var admin = new ApplicationUser
        {
            Email = normalizedEmail,
            PasswordHash =
                PasswordHasher.Hash(password),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Role = AimsRoles.Administrator,
            IsActive = true
        };

        dbContext.ApplicationUsers.Add(admin);
        await dbContext.SaveChangesAsync();
    }
}
