using APS.AIMS.Application.Auditing;
using System.Security.Claims;

namespace APS.AIMS.Api.Infrastructure;

public sealed class AuditLoggingMiddleware
{
    private static readonly HashSet<string> SafeMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Get,
            HttpMethods.Head,
            HttpMethods.Options
        };

    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuditLogService auditLogService)
    {
        if (
            SafeMethods.Contains(context.Request.Method) ||
            !context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Path.StartsWithSegments("/api/auth/login")
        )
        {
            await _next(context);
            return;
        }

        Exception? capturedException = null;

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            capturedException = exception;
            throw;
        }
        finally
        {
            try
            {
                var statusCode =
                    capturedException is null
                        ? context.Response.StatusCode
                        : StatusCodes.Status500InternalServerError;

                var userIdValue =
                    context.User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                Guid? userId =
                    Guid.TryParse(userIdValue, out var parsedUserId)
                        ? parsedUserId
                        : null;

                var (resource, resourceId) =
                    ResolveResource(context.Request.Path);

                await auditLogService.WriteAsync(
                    new WriteAuditLogRequest
                    {
                        UserId = userId,
                        UserEmail =
                            context.User.FindFirstValue(
                                ClaimTypes.Email),
                        UserDisplayName =
                            context.User.FindFirstValue(
                                ClaimTypes.Name),
                        UserRole =
                            context.User.FindFirstValue(
                                ClaimTypes.Role),
                        Action =
                            $"{context.Request.Method.ToUpperInvariant()} {resource}",
                        Resource = resource,
                        ResourceId = resourceId,
                        HttpMethod =
                            context.Request.Method.ToUpperInvariant(),
                        Path =
                            context.Request.Path.Value ?? "/api",
                        StatusCode = statusCode,
                        IpAddress =
                            context.Connection.RemoteIpAddress?.ToString(),
                        UserAgent =
                            context.Request.Headers.UserAgent.ToString(),
                        OccurredAt = DateTimeOffset.UtcNow
                    },
                    CancellationToken.None);
            }
            catch (Exception auditException)
            {
                /*
                 * Audit logging must never break the business operation.
                 * The failure is surfaced to server logs for investigation.
                 */
                _logger.LogError(
                    auditException,
                    "Failed to write APS AIMS audit log for {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);
            }
        }
    }

    private static (string Resource, string? ResourceId) ResolveResource(
        PathString path)
    {
        var segments = path.Value?
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            ?? [];

        if (segments.Length < 2)
        {
            return ("api", null);
        }

        var resource = segments[1];
        string? resourceId = null;

        foreach (var segment in segments.Skip(2))
        {
            if (Guid.TryParse(segment, out _) ||
                segment.StartsWith("AST-", StringComparison.OrdinalIgnoreCase))
            {
                resourceId = segment;
                break;
            }
        }

        return (resource, resourceId);
    }
}
