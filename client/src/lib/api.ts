import type { AuditLogEntry } from "../types/audit";
import type {
  ApplicationUser,
  AimsRole,
  AuthenticatedUser,
  LoginResponse,
} from "../types/auth";
import type {
  Asset, AssetCategory, AssetCustodyHistory, AssetFilters, AssetLocation,
  AssetTransaction, Branch, Company, CreateAssetRequest, CreateEmployeeRequest,
  Department, Employee, IssueAssetRequest, ReturnAssetRequest,
  TransferAssetRequest, UpdateAssetRequest, UpdateEmployeeRequest,
} from "../types/aims";
import type {
  AssetCalibration, AssetMaintenance,
  CompleteCalibrationRequest, CompleteMaintenanceRequest,
  StartCalibrationRequest, StartMaintenanceRequest,
} from "../types/service";
import type {
  AssetIncident,
  AssetIncidentSeverity,
  AssetIncidentType,
  AssetReportSummary,
  InventoryCampaign,
  InventoryCount,
} from "../types/v0.9";

interface ProblemDetails { title?: string; detail?: string; status?: number; }

function buildQuery(values: object) {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(values)) {
    if (
      value !== undefined &&
      value !== null &&
      value !== ""
    ) {
      params.set(key, String(value));
    }
  }

  const query = params.toString();
  return query ? `?${query}` : "";
}

async function request<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const response = await fetch(path, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(localStorage.getItem("aps-aims-access-token")
        ? {
            Authorization: `Bearer ${localStorage.getItem(
              "aps-aims-access-token",
            )}`,
          }
        : {}),
      ...options.headers,
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem("aps-aims-access-token");
      localStorage.removeItem("aps-aims-auth-user");
      window.dispatchEvent(new Event("aps-aims-unauthorized"));
    }

    let message =
      response.status === 403
        ? "Your account does not have permission to perform this action."
        : `Request failed with status ${response.status}.`;
    try {
      const problem = (await response.json()) as ProblemDetails;
      message = problem.detail || problem.title || message;
    } catch {}
    throw new Error(message);
  }

  if (response.status === 204)
    return undefined as T;

  return (await response.json()) as T;
}

export const authApi = {
  login: (email: string, password: string) =>
    request<LoginResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    }),
  me: () =>
    request<AuthenticatedUser>("/api/auth/me"),
  getUsers: () =>
    request<ApplicationUser[]>("/api/users"),
  createUser: (payload: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    role: AimsRole;
  }) =>
    request<ApplicationUser>("/api/users", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  updateUser: (
    id: string,
    payload: {
      firstName: string;
      lastName: string;
      role: AimsRole;
      isActive: boolean;
    },
  ) =>
    request<ApplicationUser>(
      `/api/users/${encodeURIComponent(id)}`,
      {
        method: "PUT",
        body: JSON.stringify(payload),
      },
    ),
  resetPassword: (id: string, newPassword: string) =>
    request<void>(
      `/api/users/${encodeURIComponent(id)}/reset-password`,
      {
        method: "POST",
        body: JSON.stringify({ newPassword }),
      },
    ),
  getAuditLogs: (limit = 200) =>
    request<AuditLogEntry[]>(
      `/api/audit-logs${buildQuery({ limit })}`,
    ),
};

export const aimsApi = {
  getAssets: (filters: AssetFilters = {}) =>
    request<Asset[]>(`/api/assets${buildQuery(filters)}`),
  getAsset: (id: string) =>
    request<Asset>(`/api/assets/${encodeURIComponent(id)}`),
  getAssetByBarcode: (barcode: string) =>
    request<Asset>(`/api/assets/barcode/${encodeURIComponent(barcode.trim())}`),
  createAsset: (payload: CreateAssetRequest) =>
    request<Asset>("/api/assets", { method: "POST", body: JSON.stringify(payload) }),
  updateAsset: (id: string, payload: UpdateAssetRequest) =>
    request<Asset>(`/api/assets/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(payload) }),

  getCustodyHistory: (assetId: string) =>
    request<AssetCustodyHistory[]>(`/api/assets/${encodeURIComponent(assetId)}/custody`),
  issueAsset: (assetId: string, payload: IssueAssetRequest) =>
    request<Asset>(`/api/assets/${encodeURIComponent(assetId)}/custody/issue`, { method: "POST", body: JSON.stringify(payload) }),
  returnAsset: (assetId: string, payload: ReturnAssetRequest) =>
    request<Asset>(`/api/assets/${encodeURIComponent(assetId)}/custody/return`, { method: "POST", body: JSON.stringify(payload) }),

  getTransactions: (assetId: string) =>
    request<AssetTransaction[]>(`/api/assets/${encodeURIComponent(assetId)}/transactions`),
  transferAsset: (assetId: string, payload: TransferAssetRequest) =>
    request<Asset>(`/api/assets/${encodeURIComponent(assetId)}/transactions/transfer`, { method: "POST", body: JSON.stringify(payload) }),

  getMaintenanceHistory: (assetId: string) =>
    request<AssetMaintenance[]>(`/api/assets/${encodeURIComponent(assetId)}/maintenance`),
  startMaintenance: (assetId: string, payload: StartMaintenanceRequest) =>
    request<Asset>(`/api/assets/${encodeURIComponent(assetId)}/maintenance/start`, { method: "POST", body: JSON.stringify(payload) }),
  completeMaintenance: (
    assetId: string,
    maintenanceId: string,
    payload: CompleteMaintenanceRequest,
  ) =>
    request<Asset>(
      `/api/assets/${encodeURIComponent(assetId)}/maintenance/${encodeURIComponent(maintenanceId)}/complete`,
      { method: "POST", body: JSON.stringify(payload) },
    ),

  getCalibrationHistory: (assetId: string) =>
    request<AssetCalibration[]>(`/api/assets/${encodeURIComponent(assetId)}/calibration`),
  startCalibration: (assetId: string, payload: StartCalibrationRequest) =>
    request<Asset>(`/api/assets/${encodeURIComponent(assetId)}/calibration/start`, { method: "POST", body: JSON.stringify(payload) }),
  completeCalibration: (
    assetId: string,
    calibrationId: string,
    payload: CompleteCalibrationRequest,
  ) =>
    request<Asset>(
      `/api/assets/${encodeURIComponent(assetId)}/calibration/${encodeURIComponent(calibrationId)}/complete`,
      { method: "POST", body: JSON.stringify(payload) },
    ),

  getEmployees: (includeInactive = false) =>
    request<Employee[]>(`/api/employees${buildQuery({ includeInactive })}`),
  createEmployee: (payload: CreateEmployeeRequest) =>
    request<Employee>("/api/employees", { method: "POST", body: JSON.stringify(payload) }),
  updateEmployee: (id: string, payload: UpdateEmployeeRequest) =>
    request<Employee>(`/api/employees/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(payload) }),
  deleteEmployee: (id: string) =>
    request<void>(`/api/employees/${encodeURIComponent(id)}`, { method: "DELETE" }),

  getCompanies: () => request<Company[]>("/api/companies"),
  getBranchesByCompany: (companyId: string) =>
    request<Branch[]>(`/api/branches/company/${companyId}`),
  getDepartmentsByBranch: (branchId: string) =>
    request<Department[]>(`/api/departments/branch/${branchId}`),
  getLocationsByBranch: (branchId: string) =>
    request<AssetLocation[]>(`/api/asset-locations/branch/${branchId}`),
  getCategories: () => request<AssetCategory[]>("/api/asset-categories"),

  getInventoryCampaigns: () =>
    request<InventoryCampaign[]>("/api/inventory-campaigns"),
  createInventoryCampaign: (payload: {
    name: string;
    description: string;
    branchId: string;
  }) =>
    request<InventoryCampaign>("/api/inventory-campaigns", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  startInventoryCampaign: (campaignId: string) =>
    request<InventoryCampaign>(
      `/api/inventory-campaigns/${encodeURIComponent(campaignId)}/start`,
      { method: "POST" },
    ),
  completeInventoryCampaign: (campaignId: string) =>
    request<InventoryCampaign>(
      `/api/inventory-campaigns/${encodeURIComponent(campaignId)}/complete`,
      { method: "POST" },
    ),
  getInventoryCounts: (campaignId: string) =>
    request<InventoryCount[]>(
      `/api/inventory-campaigns/${encodeURIComponent(campaignId)}/counts`,
    ),
  recordInventoryCount: (
    campaignId: string,
    payload: {
      barcodeValue: string;
      observedLocationId: string;
      observedCondition: string;
      notes: string;
    },
  ) =>
    request<InventoryCount>(
      `/api/inventory-campaigns/${encodeURIComponent(campaignId)}/counts`,
      {
        method: "POST",
        body: JSON.stringify(payload),
      },
    ),

  getIncidents: (openOnly = false, assetId?: string) =>
    request<AssetIncident[]>(
      `/api/incidents${buildQuery({ openOnly, assetId })}`,
    ),
  createIncident: (payload: {
    assetId: string;
    type: AssetIncidentType;
    severity: AssetIncidentSeverity;
    description: string;
    occurredAt: string | null;
  }) =>
    request<AssetIncident>("/api/incidents", {
      method: "POST",
      body: JSON.stringify(payload),
    }),
  resolveIncident: (incidentId: string, resolutionNotes: string) =>
    request<AssetIncident>(
      `/api/incidents/${encodeURIComponent(incidentId)}/resolve`,
      {
        method: "POST",
        body: JSON.stringify({ resolutionNotes }),
      },
    ),

  getReportSummary: () =>
    request<AssetReportSummary>("/api/reports/summary"),
};
