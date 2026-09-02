$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

$infra = ".\server\APS.AIMS.Infrastructure\APS.AIMS.Infrastructure.csproj"
$api = ".\server\APS.AIMS.Api\APS.AIMS.Api.csproj"

Write-Host "Adding modern Microsoft IdentityModel packages..." -ForegroundColor Cyan

dotnet package add Microsoft.IdentityModel.JsonWebTokens `
  --version 8.22.0 `
  --project $infra

if ($LASTEXITCODE -ne 0) {
    throw "Failed to add Microsoft.IdentityModel.JsonWebTokens."
}

dotnet package add Microsoft.IdentityModel.Tokens `
  --version 8.22.0 `
  --project $infra

if ($LASTEXITCODE -ne 0) {
    throw "Failed to add Microsoft.IdentityModel.Tokens."
}

dotnet package add Microsoft.AspNetCore.Authentication.JwtBearer `
  --version 10.0.11 `
  --project $api

if ($LASTEXITCODE -ne 0) {
    throw "Failed to add Microsoft.AspNetCore.Authentication.JwtBearer."
}

Write-Host ""
Write-Host "Infrastructure package references:" -ForegroundColor Cyan
dotnet package list --project $infra

if ($LASTEXITCODE -ne 0) {
    throw "Unable to list Infrastructure packages."
}

Write-Host ""
Write-Host "API package references:" -ForegroundColor Cyan
dotnet package list --project $api

if ($LASTEXITCODE -ne 0) {
    throw "Unable to list API packages."
}

Write-Host ""
Write-Host "Cleaning previous build output..." -ForegroundColor Cyan

dotnet clean .\APS.AIMS.sln

if ($LASTEXITCODE -ne 0) {
    throw "dotnet clean failed."
}

$folders = @(
    ".\server\APS.AIMS.Infrastructure\bin",
    ".\server\APS.AIMS.Infrastructure\obj",
    ".\server\APS.AIMS.Api\bin",
    ".\server\APS.AIMS.Api\obj"
)

foreach ($folder in $folders) {
    if (Test-Path $folder) {
        Remove-Item $folder -Recurse -Force
    }
}

Write-Host ""
Write-Host "Restoring..." -ForegroundColor Cyan
dotnet restore .\APS.AIMS.sln

if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed."
}

Write-Host ""
Write-Host "Building..." -ForegroundColor Cyan
dotnet build .\APS.AIMS.sln --no-restore

if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

Write-Host ""
Write-Host "JWT package/build fix completed successfully." -ForegroundColor Green
