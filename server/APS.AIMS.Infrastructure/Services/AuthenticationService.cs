using APS.AIMS.Application.Authentication;
using APS.AIMS.Infrastructure.Persistence;
using APS.AIMS.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Infrastructure.Services;

public sealed class AuthenticationService :
    IAuthenticationService
{
    private readonly AimsDbContext _dbContext;
    private readonly JwtTokenService _tokenService;

    public AuthenticationService(
        AimsDbContext dbContext,
        JwtTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                item => item.Email == email,
                cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !PasswordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _tokenService.Create(user);
    }

    public async Task<AuthenticatedUserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(
                user =>
                    user.Id == userId &&
                    user.IsActive)
            .Select(user => new AuthenticatedUserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName =
                    (user.FirstName + " " + user.LastName).Trim(),
                Role = user.Role
            })
            .FirstOrDefaultAsync(cancellationToken);
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
}
