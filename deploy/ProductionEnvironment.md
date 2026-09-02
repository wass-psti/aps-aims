# APS AIMS Production Environment

Do not store production secrets in `appsettings.json`, `.env` files committed
to Git, screenshots, issue trackers, or documentation.

Configure these as deployment-platform environment variables:

```text
ASPNETCORE_ENVIRONMENT=Production

ConnectionStrings__AimsDatabase=<SUPABASE_POSTGRES_CONNECTION_STRING>

Authentication__JwtKey=<LONG_RANDOM_SECRET>
Authentication__Issuer=APS.AIMS
Authentication__Audience=APS.AIMS.Client
```

After the first Administrator already exists in the migrated database, do not
configure `BootstrapAdmin__Password` in production unless deliberately
bootstrapping a new empty database.

## Supabase connection choice

APS AIMS uses an ASP.NET Core backend with Npgsql/EF Core.

For a long-running backend:
- Prefer the Supabase Direct connection when the deployment host supports IPv6.
- If the deployment environment is IPv4-only, use the Supabase Shared Pooler
  in Session mode.

For EF Core migrations:
- Prefer Direct connection.
- Session Pooler is the fallback for IPv4-only environments.
- Do not use Transaction mode for schema migrations.

Use the exact connection string shown by the Supabase Dashboard `Connect`
dialog. Do not manually reconstruct credentials.

## Runtime resilience

The Npgsql configuration now retries transient connection failures:
- up to 5 retries
- up to 10 seconds between retries
- 30 second command timeout

These retries are for transient network/database errors. They do not hide
invalid credentials or a missing database schema.

## Health endpoints

After deployment:

```text
GET /api/health
```

Expected healthy response:

```json
{
  "status": "Healthy",
  "database": "Connected"
}
```

Schema status:

```text
GET /api/health/database
```

Expected:

```json
{
  "connected": true,
  "pendingMigrations": 0,
  "schemaCurrent": true
}
```

The health endpoints intentionally do not return the database host, username,
password, connection string, or Supabase project identifier.
