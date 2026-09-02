using APS.AIMS.Application.Auditing;
using APS.AIMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = AimsRoles.Administrator)]
public sealed class AuditLogsController(
    IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetRecent(
        [FromQuery] int limit = 200,
        CancellationToken cancellationToken = default)
    {
        return Ok(
            await auditLogService.GetRecentAsync(
                limit,
                cancellationToken));
    }
}
