@echo off
setlocal
cd /d "%~dp0"

echo Starting APS AIMS...
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\workspace-launch.ps1"
set EXITCODE=%ERRORLEVEL%

if not "%EXITCODE%"=="0" (
    echo.
    echo APS AIMS failed to start.
    echo Exit code: %EXITCODE%
    echo.
    echo Check the logs folder inside this APS AIMS package.
    echo.
    pause
)

exit /b %EXITCODE%
