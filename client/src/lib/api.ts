import type {
  Asset,
  AssetCategory,
  AssetLocation,
  Branch,
  Company,
  CreateAssetRequest,
  Department,
} from "../types/aims";

interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
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
      // Keep the fallback message when the response body is not JSON.
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const aimsApi = {
  getAssets: () => request<Asset[]>("/api/assets"),

  createAsset: (payload: CreateAssetRequest) =>
    request<Asset>("/api/assets", {
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
