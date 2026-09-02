using APS.AIMS.Api.Infrastructure;
using APS.AIMS.Infrastructure;
using APS.AIMS.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var workspaceMode =
    builder.Environment.IsEnvironment("Workspace");

if (workspaceMode)
{
    /*
     * Workspace packages remain local-first for now.
     * Reuse the existing ASP.NET User Secrets on the development machine
     * without placing database/JWT secrets inside the ZIP.
     */
    builder.Configuration.AddUserSecrets(
        Assembly.GetExecutingAssembly(),
        optional: true);
}


builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add(
            new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

if (!workspaceMode)
{
    var jwtKey =
        builder.Configuration["Authentication:JwtKey"]
        ?? throw new InvalidOperationException(
            "Authentication:JwtKey is required.");

    var jwtIssuer =
        builder.Configuration["Authentication:Issuer"]
        ?? "APS.AIMS";

    var jwtAudience =
        builder.Configuration["Authentication:Audience"]
        ?? "APS.AIMS.Client";

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
        });
}

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!builder.Configuration.GetValue<bool>(
    "Workspace:DisableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

if (workspaceMode)
{
    /*
     * Authentication is performed once by the outer Workspace application.
     * APS AIMS therefore trusts the local Workspace launch boundary and
     * supplies an internal Administrator principal for its existing
     * authorization policies.
     *
     * The Workspace launcher binds APS AIMS to 127.0.0.1 only.
     */
    app.UseMiddleware<WorkspaceIdentityMiddleware>();
}
else
{
    app.UseAuthentication();
}

app.UseMiddleware<AuditLoggingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

if (!workspaceMode)
{
    await app.Services.EnsureAimsIdentitySeededAsync();
}

app.Run();
