using System.Security.Cryptography;

namespace APS.AIMS.Infrastructure.Security;

internal static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 210_000;

    public static string Hash(string password)
    {
        ValidatePassword(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            ".",
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static bool Verify(
        string password,
        string encodedHash)
    {
        try
        {
            var parts = encodedHash.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);

            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);

            return CryptographicOperations.FixedTimeEquals(
                actual,
                expected);
        }
        catch
        {
            return false;
        }
    }

    public static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < 12)
        {
            throw new ArgumentException(
                "Password must contain at least 12 characters.");
        }
    }
}
