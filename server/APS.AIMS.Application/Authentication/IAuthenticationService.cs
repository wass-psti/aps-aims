namespace APS.AIMS.Application.Authentication;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUserDto?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
