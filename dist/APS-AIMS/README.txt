APS AIMS Workspace Package
Version: v1.0.0
Runtime: win-x64

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
