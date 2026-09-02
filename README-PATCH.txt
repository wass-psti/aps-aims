APS AIMS v1.0.0 — Workspace Login Bypass

Purpose
-------
APS AIMS no longer asks for a second login when launched from the existing
Workspace application.

The outer Workspace login becomes the user-facing authentication boundary.

Behavior
--------
Workspace package:
- No APS AIMS Login screen.
- No APS AIMS Sign out button.
- No Users & Roles page.
- /api/auth/* is disabled.
- /api/users/* is disabled.
- Existing APS AIMS role-protected endpoints continue to work because Workspace
  mode supplies an internal Administrator principal.
- Audit logs record the generic identity "Workspace User".
- JWT secrets and BootstrapAdmin credentials are no longer required merely to
  launch the Workspace package.
- APS AIMS remains bound to 127.0.0.1:5175 by the Workspace launcher.

Normal Development/standalone source mode:
- Existing APS AIMS JWT login remains available.
- Existing Users & Roles remains available.
- This provides a rollback/testing path without deleting authentication tables
  or creating a destructive database migration.

Why authentication tables are not deleted
-----------------------------------------
Removing the ApplicationUsers table and auth migrations would be an unnecessary
database/schema risk. The internal auth system is simply dormant in Workspace
mode. It can be removed permanently later after the Workspace integration is
fully validated.

Audit limitation
----------------
The current Workspace launcher does not provide APS AIMS with the identity of
the person who logged into the outer Workspace.

Therefore APS AIMS can currently record:
    Workspace User / Administrator

but cannot yet distinguish Alice vs Bob inside its own Audit Logs.

If Workspace Manager later exposes a trusted SSO/session identity to child apps,
APS AIMS can map that identity into its audit trail without restoring a second
login screen.

Apply
-----
Extract into:

C:\Users\lucke\source\repos\aps-aims

Allow overwrite.

Then rebuild the Workspace package:

powershell -ExecutionPolicy Bypass -File .\scripts\build-workspace-package.ps1

Launch the generated package:

.\dist\APS-AIMS\launch-windows.cmd

Expected:
1. Browser opens APS AIMS directly.
2. No APS AIMS login page.
3. Header says "Workspace Access".
4. No "Sign out" button.
5. No "Users & Roles" navigation item.
6. Existing asset/inventory/incident/report features remain available.
7. Audit Logs remains available for operational verification.

No database migration is required.
Supabase remains deferred.
