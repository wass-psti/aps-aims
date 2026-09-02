namespace APS.AIMS.Domain.Security;

public static class AimsAuthorization
{
    public const string Managers =
        AimsRoles.Administrator + "," +
        AimsRoles.AssetManager;

    public const string Operators =
        AimsRoles.Administrator + "," +
        AimsRoles.AssetManager + "," +
        AimsRoles.Custodian;

    public const string AllAuthenticated =
        AimsRoles.Administrator + "," +
        AimsRoles.AssetManager + "," +
        AimsRoles.Custodian + "," +
        AimsRoles.Viewer;

    public const string CanManageAssets = Managers;
    public const string CanManageMasterData = Managers;
    public const string CanManageEmployees = Managers;
    public const string CanOperateCustody = Operators;
    public const string CanTransferAssets = Operators;
    public const string CanManageService = Managers;
    public const string CanManageInventory = Managers;
    public const string CanCountInventory = Operators;
    public const string CanReportIncidents = Operators;
    public const string CanResolveIncidents = Managers;
}
