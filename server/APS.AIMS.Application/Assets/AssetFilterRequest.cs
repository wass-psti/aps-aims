using APS.AIMS.Domain.Enums;

namespace APS.AIMS.Application.Assets;

public sealed class AssetFilterRequest
{
    public string? Search { get; init; }

    public Guid? CategoryId { get; init; }

    public Guid? CompanyId { get; init; }

    public Guid? BranchId { get; init; }

    public Guid? DepartmentId { get; init; }

    public Guid? LocationId { get; init; }

    public AssetStatus? Status { get; init; }

    public AssetCondition? Condition { get; init; }

    public bool IncludeArchived { get; init; }
}
