import type {
  Asset,
  AssetCategory,
  AssetCustodyHistory,
  AssetFilters,
  AssetLocation,
  Branch,
  Company,
  CreateAssetRequest,
  CreateEmployeeRequest,
  Department,
  Employee,
  IssueAssetRequest,
  ReturnAssetRequest,
  UpdateAssetRequest,
} from "../types/aims";

interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
}

function buildQuery(
  values: Record<string, string | number | boolean | null | undefined>,
) {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(values)) {
    if (value !== undefined && value !== null && value !== "") {
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
      ...options.headers,
    },
  });

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`;

    try {
      const problem = (await response.json()) as ProblemDetails;
      message = problem.detail || problem.title || message;
    } catch {
      // Keep fallback message.
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const aimsApi = {
  getAssets: (filters: AssetFilters = {}) =>
    request<Asset[]>(`/api/assets${buildQuery(filters)}`),

  getAsset: (id: string) =>
    request<Asset>(`/api/assets/${encodeURIComponent(id)}`),

  createAsset: (payload: CreateAssetRequest) =>
    request<Asset>("/api/assets", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  updateAsset: (id: string, payload: UpdateAssetRequest) =>
    request<Asset>(`/api/assets/${encodeURIComponent(id)}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    }),

  getCustodyHistory: (assetId: string) =>
    request<AssetCustodyHistory[]>(
      `/api/assets/${encodeURIComponent(assetId)}/custody`,
    ),

  issueAsset: (assetId: string, payload: IssueAssetRequest) =>
    request<Asset>(
      `/api/assets/${encodeURIComponent(assetId)}/custody/issue`,
      {
        method: "POST",
        body: JSON.stringify(payload),
      },
    ),

  returnAsset: (assetId: string, payload: ReturnAssetRequest) =>
    request<Asset>(
      `/api/assets/${encodeURIComponent(assetId)}/custody/return`,
      {
        method: "POST",
        body: JSON.stringify(payload),
      },
    ),

  getEmployees: () => request<Employee[]>("/api/employees"),

  createEmployee: (payload: CreateEmployeeRequest) =>
    request<Employee>("/api/employees", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  getCompanies: () => request<Company[]>("/api/companies"),

  getBranchesByCompany: (companyId: string) =>
    request<Branch[]>(`/api/branches/company/${companyId}`),

  getDepartmentsByBranch: (branchId: string) =>
    request<Department[]>(`/api/departments/branch/${branchId}`),

  getLocationsByBranch: (branchId: string) =>
    request<AssetLocation[]>(`/api/asset-locations/branch/${branchId}`),

  getCategories: () =>
    request<AssetCategory[]>("/api/asset-categories"),
};
