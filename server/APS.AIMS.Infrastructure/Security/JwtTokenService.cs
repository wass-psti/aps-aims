using APS.AIMS.Application.Authentication;
using APS.AIMS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace APS.AIMS.Infrastructure.Security;

public sealed class JwtTokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IConfiguration configuration)
    {
        _key =
            configuration["Authentication:JwtKey"]
            ?? throw new InvalidOperationException(
                "Authentication:JwtKey is required.");

        if (_key.Length < 32)
        {
            throw new InvalidOperationException(
                "Authentication:JwtKey must contain at least 32 characters.");
        }

        _issuer =
            configuration["Authentication:Issuer"]
            ?? "APS.AIMS";

        _audience =
            configuration["Authentication:Audience"]
            ?? "APS.AIMS.Client";
    }

    public LoginResponse Create(ApplicationUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddHours(8);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),
            new Claim(
                ClaimTypes.Name,
                user.DisplayName),
            new Claim(
                ClaimTypes.Email,
                user.Email),
            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var credentials =
            new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_key)),
                SecurityAlgorithms.HmacSha256);

        var descriptor =
            new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _audience,
                Subject = new ClaimsIdentity(claims),
                NotBefore = now.UtcDateTime,
                Expires = expiresAt.UtcDateTime,
                SigningCredentials = credentials
            };

        var accessToken =
            new JsonWebTokenHandler()
                .CreateToken(descriptor);

        return new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            User = new AuthenticatedUserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role
            }
        };
    }
}
