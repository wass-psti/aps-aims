$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

if ([string]::IsNullOrWhiteSpace(
    $env:ConnectionStrings__AimsDatabase)) {
    throw @"
ConnectionStrings__AimsDatabase is not set.

Set it to the connection string copied from Supabase Dashboard > Connect:

`$env:ConnectionStrings__AimsDatabase = "postgresql://..."
"@
}

Write-Host "Applying APS AIMS EF migrations to Supabase..." -ForegroundColor Cyan

dotnet ef database update `
  --project .\server\APS.AIMS.Infrastructure `
  --startup-project .\server\APS.AIMS.Api

if ($LASTEXITCODE -ne 0) {
    throw "Supabase migration failed."
}

Write-Host ""
Write-Host "Verifying database migration state..." -ForegroundColor Cyan

$migrationOutput = dotnet ef migrations list `
  --project .\server\APS.AIMS.Infrastructure `
  --startup-project .\server\APS.AIMS.Api `
  2>&1

$migrationOutput | ForEach-Object { Write-Host $_ }

$badOutput = $migrationOutput |
    Select-String -Pattern `
      "Unable to determine which migrations have been applied|error occurred while accessing the database|Format of the initialization string"

if ($LASTEXITCODE -ne 0 -or $badOutput) {
    throw "Database migration verification failed."
}

Write-Host ""
Write-Host "Supabase schema deployment completed successfully." -ForegroundColor Green
