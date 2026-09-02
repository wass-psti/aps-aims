$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$appDir = Join-Path $root "app"
$exe = Join-Path $appDir "APS.AIMS.Api.exe"
$logsDir = Join-Path $root "logs"
$stdoutLog = Join-Path $logsDir "aps-aims.stdout.log"
$stderrLog = Join-Path $logsDir "aps-aims.stderr.log"

$liveUrl = "http://127.0.0.1:5175/api/health/live"
$dbHealthUrl = "http://127.0.0.1:5175/api/health"
$appUrl = "http://127.0.0.1:5175"

New-Item $logsDir -ItemType Directory -Force | Out-Null

function Write-RecentLogs {
    Write-Host ""
    Write-Host "---- APS AIMS stdout ----" -ForegroundColor Yellow
    if (Test-Path $stdoutLog) {
        Get-Content $stdoutLog -Tail 40
    }
    else {
        Write-Host "(no stdout log)"
    }

    Write-Host ""
    Write-Host "---- APS AIMS stderr ----" -ForegroundColor Yellow
    if (Test-Path $stderrLog) {
        Get-Content $stderrLog -Tail 40
    }
    else {
        Write-Host "(no stderr log)"
    }
}

function Test-Url([string]$url) {
    try {
        $response = Invoke-WebRequest `
            -Uri $url `
            -UseBasicParsing `
            -TimeoutSec 2

        return $response.StatusCode -ge 200 -and
               $response.StatusCode -lt 500
    }
    catch {
        return $false
    }
}

if (-not (Test-Path $exe)) {
    throw @"
APS AIMS published runtime was not found:

$exe

Build the Workspace package first with:
scripts\build-workspace-package.ps1
"@
}

# If APS AIMS is already running, just open it.
if (Test-Url $liveUrl) {
    Write-Host "APS AIMS is already running." -ForegroundColor Green
    Start-Process $appUrl
    exit 0
}

# Clear previous logs so this run is easy to diagnose.
Remove-Item $stdoutLog -Force -ErrorAction SilentlyContinue
Remove-Item $stderrLog -Force -ErrorAction SilentlyContinue

$env:ASPNETCORE_ENVIRONMENT = "Workspace"
$env:ASPNETCORE_URLS = "http://127.0.0.1:5175"
$env:Workspace__DisableHttpsRedirection = "true"

Write-Host "Launching APS AIMS local server..." -ForegroundColor Cyan

$process = Start-Process `
    -FilePath $exe `
    -WorkingDirectory $appDir `
    -RedirectStandardOutput $stdoutLog `
    -RedirectStandardError $stderrLog `
    -PassThru

$deadline = (Get-Date).AddSeconds(45)

while ((Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 500

    if ($process.HasExited) {
        Write-RecentLogs
        throw "APS AIMS exited before the local server became ready."
    }

    if (Test-Url $liveUrl) {
        Write-Host "APS AIMS server is ready." -ForegroundColor Green

        # Database health is informative, but does not block the browser launch.
        if (Test-Url $dbHealthUrl) {
            Write-Host "Local PostgreSQL connection is healthy." -ForegroundColor Green
        }
        else {
            Write-Warning @"
APS AIMS started, but the database health check is not healthy.
The browser will still open so the actual application error can be inspected.
Check that local PostgreSQL is running and that APS AIMS User Secrets are configured.
"@
        }

        Start-Process $appUrl
        exit 0
    }
}

try {
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
}
catch {
}

Write-RecentLogs

throw @"
APS AIMS did not become ready within 45 seconds.

Common causes:
- port 5175 is already occupied;
- local PostgreSQL is stopped;
- ASP.NET User Secrets are unavailable;
- the published package is incomplete.

Review:
$stdoutLog
$stderrLog
"@
