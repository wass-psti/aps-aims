$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

Write-Host "Installing APS AIMS v1.0.0 authentication dependencies..." -ForegroundColor Cyan

dotnet add .\server\APS.AIMS.Infrastructure\APS.AIMS.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt

dotnet add .\server\APS.AIMS.Api\APS.AIMS.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.11

Write-Host ""
Write-Host "Restoring solution..." -ForegroundColor Cyan
dotnet restore .\APS.AIMS.sln

Write-Host ""
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build .\APS.AIMS.sln

Write-Host ""
Write-Host "Authentication build-fix dependency installation completed." -ForegroundColor Green
