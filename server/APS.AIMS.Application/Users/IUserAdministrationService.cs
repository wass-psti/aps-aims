namespace APS.AIMS.Application.Users;

public interface IUserAdministrationService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<UserDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserDto> UpdateAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        Guid userId,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken = default);
}
