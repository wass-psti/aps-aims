# APS AIMS v1.0.0 — Workspace Manager Compatibility

Supabase integration is deferred.

APS AIMS keeps its current application architecture:

- ASP.NET Core 10 backend
- React + TypeScript frontend
- EF Core
- current local PostgreSQL database
- current APS AIMS authentication/roles/audit trail

The compatibility layer follows the same standalone package convention used by
the earlier Workspace Manager applications:

- `app-manifest.json`
- `launch-windows.cmd`
- `launch-macos.command`
- single local application URL
- frontend and API served from one process
- no separate frontend terminal
- no separate backend terminal
- package can be ZIP-compressed for Workspace Manager

## Single-port architecture

Development can still use Vite normally.

For a Workspace package, `npm run build` creates the React production files.
The packaging script copies them into the ASP.NET API's `wwwroot`.

ASP.NET then serves:

- `/` and React routes -> frontend
- `/api/*` -> APS AIMS API

on:

`http://127.0.0.1:5175`

This mirrors the earlier Workspace Manager pattern where the end user launches
one application instead of manually running frontend/backend processes.

## Build a Windows package

Stop any running APS.AIMS.Api process first.

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-workspace-package.ps1
```

Output:

`dist\APS-AIMS\`

Test it:

```powershell
.\dist\APS-AIMS\launch-windows.cmd
```

If the browser opens and login works, create the ZIP:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-workspace-zip.ps1
```

Output:

`dist\APS-AIMS-v1.0.0.zip`

## macOS package structure

The same convention is included.

To create an Apple Silicon runtime:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-workspace-package.ps1 -Runtime osx-arm64
```

For Intel macOS:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-workspace-package.ps1 -Runtime osx-x64
```

The package uses `launch-macos.command`.

macOS database configuration still needs to be addressed before final
cross-machine release because the current local PostgreSQL/User Secrets belong
to the development environment.

## Current boundary

This step makes APS AIMS structurally compatible with the Workspace Manager
package/launcher model.

It deliberately does NOT:

- connect to Supabase;
- rewrite the .NET backend into Node/Express;
- replace PostgreSQL with SQLite;
- embed database/JWT passwords into the package;
- declare APS AIMS final-production complete.

That avoids destabilizing the already-working v0.1-v1.0 backend just to match a
launcher convention.

After this compatibility layer is tested, the next decision is database
portability for other computers:
1. keep local PostgreSQL and provide controlled configuration/bootstrap; or
2. introduce an embedded/local database mode with a separate provider design.

That decision should be made separately from Workspace Manager compatibility.
