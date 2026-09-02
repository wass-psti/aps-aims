using Npgsql;

namespace APS.AIMS.Infrastructure.Configuration;

internal static class PostgresConnectionStringNormalizer
{
    public static string Normalize(string value)
    {
        var trimmed = value.Trim();

        if (
            !trimmed.StartsWith(
                "postgres://",
                StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith(
                "postgresql://",
                StringComparison.OrdinalIgnoreCase))
        {
            // Local development and manually supplied Npgsql strings already
            // use key=value format and should pass through unchanged.
            return trimmed;
        }

        if (!Uri.TryCreate(
            trimmed,
            UriKind.Absolute,
            out var uri))
        {
            throw new InvalidOperationException(
                "The PostgreSQL URI is invalid. Copy the connection string again from Supabase Dashboard > Connect.");
        }

        var userInfo = uri.UserInfo.Split(
            ':',
            2,
            StringSplitOptions.None);

        if (userInfo.Length == 0 ||
            string.IsNullOrWhiteSpace(userInfo[0]))
        {
            throw new InvalidOperationException(
                "The PostgreSQL URI does not contain a username.");
        }

        var username =
            Uri.UnescapeDataString(userInfo[0]);

        var password =
            userInfo.Length > 1
                ? Uri.UnescapeDataString(userInfo[1])
                : string.Empty;

        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                "The PostgreSQL URI does not contain a database password.");
        }

        var database =
            uri.AbsolutePath.Trim('/');

        if (string.IsNullOrWhiteSpace(database))
        {
            database = "postgres";
        }

        var builder =
            new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port =
                    uri.IsDefaultPort
                        ? 5432
                        : uri.Port,
                Database = database,
                Username = username,
                Password = password,
                SslMode = SslMode.Require,
                Pooling = true,
                Timeout = 15,
                CommandTimeout = 30
            };

        return builder.ConnectionString;
    }
}
