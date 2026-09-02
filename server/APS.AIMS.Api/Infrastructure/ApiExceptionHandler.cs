using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace APS.AIMS.Api.Infrastructure;

public sealed class ApiExceptionHandler :
    IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Invalid request"),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized"),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Not found"),
            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Operation conflict"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Server error")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
