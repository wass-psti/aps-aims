$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

if ([string]::IsNullOrWhiteSpace(
    $env:ConnectionStrings__AimsDatabase)) {
    throw @"
ConnectionStrings__AimsDatabase is not set in this PowerShell session.

Use the Supabase connection string copied from Dashboard > Connect:

`$env:ConnectionStrings__AimsDatabase = "postgresql://..."
"@
}

Write-Host "Building APS AIMS..." -ForegroundColor Cyan
dotnet build .\APS.AIMS.sln

if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

Write-Host ""
Write-Host "Testing configured PostgreSQL connection..." -ForegroundColor Cyan

$output = dotnet ef database update `
  --project .\server\APS.AIMS.Infrastructure `
  --startup-project .\server\APS.AIMS.Api `
  2>&1

$output | ForEach-Object { Write-Host $_ }

if ($LASTEXITCODE -ne 0) {
    throw "Unable to connect to or update the configured Supabase PostgreSQL database."
}

Write-Host ""
Write-Host "Supabase PostgreSQL connection and migration check succeeded." -ForegroundColor Green
