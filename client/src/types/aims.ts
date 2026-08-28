export type AssetStatus =
  | "Available"
  | "Reserved"
  | "Issued"
  | "ProjectAssigned"
  | "InTransit"
  | "UnderInspection"
  | "UnderMaintenance"
  | "UnderCalibration"
  | "Quarantined"
  | "Missing"
  | "LostOrStolen"
  | "Retired"
  | "Disposed";

export type AssetCondition =
  | "New"
  | "Excellent"
  | "Good"
  | "Fair"
  | "Damaged"
  | "Unserviceable"
  | "ForDisposal";

export interface Company {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface Branch {
  id: string;
  code: string;
  name: string;
  companyId: string;
  companyName: string;
  isActive: boolean;
}

export interface Department {
  id: string;
  code: string;
  name: string;
  branchId: string;
  branchName: string;
  isActive: boolean;
}

export interface AssetLocation {
  id: string;
  code: string;
  name: string;
  branchId: string;
  branchName: string;
  parentLocationId?: string | null;
  parentLocationName?: string | null;
  isActive: boolean;
}

export interface AssetCategory {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  parentCategoryId?: string | null;
  parentCategoryName?: string | null;
  calibrationRequired: boolean;
  maintenanceRequired: boolean;
  approvalRequired: boolean;
  isActive: boolean;
}

export interface Asset {
  id: string;
  assetId: string;
  barcodeValue: string;
  name: string;
  shortDescription?: string | null;

  categoryId: string;
  categoryName: string;

  serialNumber?: string | null;
  manufacturer?: string | null;
  model?: string | null;
  partNumber?: string | null;
  legacyAssetId?: string | null;

  acquisitionCost?: number | null;
  currency?: string | null;

  companyId: string;
  companyName: string;

  branchId: string;
  branchName: string;

  departmentId?: string | null;
  departmentName?: string | null;

  currentLocationId: string;
  currentLocationName: string;

  currentCustodianId?: string | null;
  currentCustodianName?: string | null;

  status: AssetStatus;
  condition: AssetCondition;

  isArchived: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateAssetRequest {
  name: string;
  shortDescription: string;

  categoryId: string;

  serialNumber: string;
  manufacturer: string;
  model: string;
  partNumber: string;
  legacyAssetId: string;

  acquisitionCost: number | null;
  currency: string;

  companyId: string;
  branchId: string;
  departmentId: string | null;
  currentLocationId: string;

  currentCustodianId: string | null;
  barcodeValue: string | null;

  status: AssetStatus;
  condition: AssetCondition;
}

export interface UpdateAssetRequest {
  name: string;
  shortDescription: string;

  categoryId: string;

  serialNumber: string;
  manufacturer: string;
  model: string;
  partNumber: string;
  legacyAssetId: string;

  acquisitionCost: number | null;
  currency: string;
}

export interface AssetFilters {
  search?: string;
  categoryId?: string;
  companyId?: string;
  branchId?: string;
  departmentId?: string;
  locationId?: string;
  status?: AssetStatus | "";
  condition?: AssetCondition | "";
}
