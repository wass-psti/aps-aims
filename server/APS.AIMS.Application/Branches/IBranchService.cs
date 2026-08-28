namespace APS.AIMS.Application.Branches;

public interface IBranchService
{
    Task<IReadOnlyList<BranchDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<BranchDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchDto>> GetByCompanyAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<BranchDto> CreateAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<BranchDto?> UpdateAsync(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}