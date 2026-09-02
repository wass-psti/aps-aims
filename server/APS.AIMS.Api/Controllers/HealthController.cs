using APS.AIMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APS.AIMS.Api.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public sealed class HealthController(
    AimsDbContext dbContext) : ControllerBase
{

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(
            new
            {
                status = "Alive",
                utc = DateTimeOffset.UtcNow
            });
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        try
        {
            var canConnect =
                await dbContext.Database.CanConnectAsync(
                    cancellationToken);

            if (!canConnect)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        status = "Unhealthy",
                        database = "Disconnected",
                        utc = DateTimeOffset.UtcNow
                    });
            }

            return Ok(
                new
                {
                    status = "Healthy",
                    database = "Connected",
                    utc = DateTimeOffset.UtcNow
                });
        }
        catch
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    status = "Unhealthy",
                    database = "Disconnected",
                    utc = DateTimeOffset.UtcNow
                });
        }
    }

    [HttpGet("database")]
    public async Task<IActionResult> Database(
        CancellationToken cancellationToken)
    {
        try
        {
            var applied =
                await dbContext.Database
                    .GetAppliedMigrationsAsync(
                        cancellationToken);

            var pending =
                await dbContext.Database
                    .GetPendingMigrationsAsync(
                        cancellationToken);

            return Ok(
                new
                {
                    connected = true,
                    appliedMigrations =
                        applied.Count(),
                    pendingMigrations =
                        pending.Count(),
                    schemaCurrent =
                        !pending.Any(),
                    utc = DateTimeOffset.UtcNow
                });
        }
        catch
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    connected = false,
                    schemaCurrent = false,
                    utc = DateTimeOffset.UtcNow
                });
        }
    }
}
