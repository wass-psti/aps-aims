namespace APS.AIMS.Domain.Security;

public static class AimsRoles
{
    public const string Administrator = "Administrator";
    public const string AssetManager = "AssetManager";
    public const string Custodian = "Custodian";
    public const string Viewer = "Viewer";

    public static readonly string[] All =
    [
        Administrator,
        AssetManager,
        Custodian,
        Viewer
    ];
}
