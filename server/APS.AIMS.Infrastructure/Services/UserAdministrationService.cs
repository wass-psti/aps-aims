using APS.AIMS.Application.Common;
using APS.AIMS.Application.Users;
using APS.AIMS.Domain.Entities;
using APS.AIMS.Domain.Security;
using APS.AIMS.Infrastructure.Persistence;
using APS.AIMS.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class UserAdministrationService :
    IUserAdministrationService
{
    private readonly AimsDbContext _dbContext;

    public UserAdministrationService(
        AimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApplicationUsers
            .AsNoTracking()
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName =
                    (user.FirstName + " " + user.LastName).Trim(),
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var role = ValidateRole(request.Role);

        if (await _dbContext.ApplicationUsers.AnyAsync(
            user => user.Email == email,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = email,
            PasswordHash =
                PasswordHasher.Hash(request.Password),
            FirstName =
                TextNormalizer.Required(request.FirstName),
            LastName =
                TextNormalizer.Required(request.LastName),
            Role = role,
            IsActive = true
        };

        _dbContext.ApplicationUsers.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                item => item.Id == userId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "User was not found.");

        user.FirstName =
            TextNormalizer.Required(request.FirstName);
        user.LastName =
            TextNormalizer.Required(request.LastName);
        user.Role = ValidateRole(request.Role);
        user.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task ResetPasswordAsync(
        Guid userId,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                item => item.Id == userId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "User was not found.");

        user.PasswordHash =
            PasswordHasher.Hash(request.NewPassword);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string value)
    {
        var email = value.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) ||
            !email.Contains('@'))
        {
            throw new ArgumentException(
                "A valid email address is required.");
        }

        return email;
    }

    private static string ValidateRole(string value)
    {
        var role = value.Trim();

        if (!AimsRoles.All.Contains(
            role,
            StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Selected role is invalid.");
        }

        return role;
    }

    private static UserDto ToDto(
        ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
