$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$packageRoot = Join-Path $repoRoot "dist\APS-AIMS"
$zipPath = Join-Path $repoRoot "dist\APS-AIMS-v1.0.0.zip"

if (-not (Test-Path $packageRoot)) {
    throw "dist\APS-AIMS does not exist. Run build-workspace-package.ps1 first."
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive `
    -Path (Join-Path $packageRoot "*") `
    -DestinationPath $zipPath `
    -CompressionLevel Optimal

Write-Host "Created: $zipPath" -ForegroundColor Green
