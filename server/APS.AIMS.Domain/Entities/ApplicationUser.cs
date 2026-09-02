namespace APS.AIMS.Domain.Entities;

public class ApplicationUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } =
        DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAt { get; set; }

    public string DisplayName =>
        $"{FirstName} {LastName}".Trim();
}
