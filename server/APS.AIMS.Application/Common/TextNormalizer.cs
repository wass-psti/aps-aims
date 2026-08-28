namespace APS.AIMS.Application.Common;

public static class TextNormalizer
{
    public static string? Optional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    public static string Required(string value)
    {
        return value.Trim();
    }

    public static string Code(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    public static string? CodeOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToUpperInvariant();
    }
}
