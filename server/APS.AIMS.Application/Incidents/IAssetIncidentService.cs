namespace APS.AIMS.Application.Incidents;

public interface IAssetIncidentService
{
    Task<IReadOnlyList<AssetIncidentDto>> GetAllAsync(
        bool openOnly,
        Guid? assetId,
        CancellationToken cancellationToken = default);

    Task<AssetIncidentDto> CreateAsync(
        CreateAssetIncidentRequest request,
        CancellationToken cancellationToken = default);

    Task<AssetIncidentDto> ResolveAsync(
        Guid incidentId,
        ResolveAssetIncidentRequest request,
        CancellationToken cancellationToken = default);
}
