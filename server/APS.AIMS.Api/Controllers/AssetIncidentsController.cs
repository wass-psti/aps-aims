using APS.AIMS.Application.Incidents;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/incidents")]
public sealed class AssetIncidentsController(
    IAssetIncidentService incidentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetIncidentDto>>> GetAll(
        [FromQuery] bool openOnly,
        [FromQuery] Guid? assetId,
        CancellationToken cancellationToken)
    {
        return Ok(
            await incidentService.GetAllAsync(
                openOnly,
                assetId,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanReportIncidents)]
    [HttpPost]
    public async Task<ActionResult<AssetIncidentDto>> Create(
        CreateAssetIncidentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await incidentService.CreateAsync(
                request,
                cancellationToken));
    }

    [Authorize(Roles = AimsAuthorization.CanResolveIncidents)]
    [HttpPost("{incidentId:guid}/resolve")]
    public async Task<ActionResult<AssetIncidentDto>> Resolve(
        Guid incidentId,
        ResolveAssetIncidentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await incidentService.ResolveAsync(
                incidentId,
                request,
                cancellationToken));
    }
}
