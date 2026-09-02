param(
    [ValidateSet("win-x64", "osx-x64", "osx-arm64")]
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

$clientDir = Join-Path $repoRoot "client"
$apiDir = Join-Path $repoRoot "server\APS.AIMS.Api"
$webRoot = Join-Path $apiDir "wwwroot"
$clientDist = Join-Path $clientDir "dist"

$packageRoot = Join-Path $repoRoot "dist\APS-AIMS"
$packageApp = Join-Path $packageRoot "app"
$packageScripts = Join-Path $packageRoot "scripts"

Write-Host "Building APS AIMS frontend for Workspace mode..." -ForegroundColor Cyan
Push-Location $clientDir

$previousWorkspaceMode =
    $env:VITE_WORKSPACE_MODE

try {
    $env:VITE_WORKSPACE_MODE = "true"

    npm run build

    if ($LASTEXITCODE -ne 0) {
        throw "Frontend build failed."
    }
}
finally {
    if ($null -eq $previousWorkspaceMode) {
        Remove-Item Env:VITE_WORKSPACE_MODE `
            -ErrorAction SilentlyContinue
    }
    else {
        $env:VITE_WORKSPACE_MODE =
            $previousWorkspaceMode
    }

    Pop-Location
}

if (-not (Test-Path (Join-Path $clientDist "index.html"))) {
    throw "Vite did not produce client\dist\index.html."
}

Write-Host "Preparing ASP.NET static web root..." -ForegroundColor Cyan

if (Test-Path $webRoot) {
    Remove-Item $webRoot -Recurse -Force
}

New-Item $webRoot -ItemType Directory -Force | Out-Null
Copy-Item (Join-Path $clientDist "*") $webRoot -Recurse -Force

Write-Host "Publishing APS AIMS runtime for $Runtime..." -ForegroundColor Cyan

if (Test-Path $packageRoot) {
    Remove-Item $packageRoot -Recurse -Force
}

New-Item $packageApp -ItemType Directory -Force | Out-Null
New-Item $packageScripts -ItemType Directory -Force | Out-Null

dotnet publish `
    .\server\APS.AIMS.Api\APS.AIMS.Api.csproj `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $packageApp

if ($LASTEXITCODE -ne 0) {
    throw "ASP.NET publish failed."
}

Copy-Item .\app-manifest.json $packageRoot -Force
Copy-Item .\launch-windows.cmd $packageRoot -Force
Copy-Item .\launch-macos.command $packageRoot -Force
Copy-Item .\scripts\workspace-launch.ps1 $packageScripts -Force
Copy-Item .\scripts\workspace-launch.sh $packageScripts -Force

$readme = @"
APS AIMS Workspace Package
Version: v1.0.0
Runtime: $Runtime

Launch:
Windows  -> launch-windows.cmd
macOS    -> launch-macos.command

Local address:
http://127.0.0.1:5175

This package currently uses the existing APS AIMS local PostgreSQL database.
Supabase is NOT required.

On the current development computer, APS AIMS Workspace mode reuses the
existing ASP.NET User Secrets so database and JWT secrets are not embedded in
the ZIP.

Final cross-computer distribution will receive a separate configuration/
database portability step after Workspace Manager compatibility is validated.
"@

$readme | Set-Content `
    (Join-Path $packageRoot "README.txt") `
    -Encoding UTF8

Write-Host ""
Write-Host "Workspace package created:" -ForegroundColor Green
Write-Host $packageRoot
