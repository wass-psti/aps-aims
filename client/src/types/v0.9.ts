import type { AssetCondition } from "./aims";

export type InventoryCampaignStatus =
  | "Draft"
  | "InProgress"
  | "Completed"
  | "Cancelled";

export type InventoryCountResult =
  | "Matched"
  | "LocationMismatch"
  | "ConditionMismatch"
  | "LocationAndConditionMismatch";

export interface InventoryCampaign {
  id: string;
  name: string;
  description?: string | null;
  branchId: string;
  branchName: string;
  status: InventoryCampaignStatus;
  countedAssets: number;
  createdAt: string;
  startedAt?: string | null;
  completedAt?: string | null;
}

export interface InventoryCount {
  id: string;
  campaignId: string;
  assetId: string;
  assetBusinessId: string;
  assetName: string;
  barcodeValue: string;
  systemLocationId: string;
  systemLocationName: string;
  observedLocationId: string;
  observedLocationName: string;
  systemCondition: AssetCondition;
  observedCondition: AssetCondition;
  result: InventoryCountResult;
  notes?: string | null;
  countedAt: string;
}

export type AssetIncidentType =
  | "Damage"
  | "Missing"
  | "LostOrStolen"
  | "Other";

export type AssetIncidentSeverity =
  | "Low"
  | "Medium"
  | "High"
  | "Critical";

export type AssetIncidentStatus =
  | "Open"
  | "Resolved";

export interface AssetIncident {
  id: string;
  assetId: string;
  assetBusinessId: string;
  assetName: string;
  type: AssetIncidentType;
  severity: AssetIncidentSeverity;
  status: AssetIncidentStatus;
  description: string;
  occurredAt: string;
  reportedAt: string;
  resolutionNotes?: string | null;
  resolvedAt?: string | null;
}

export interface ReportCount {
  label: string;
  count: number;
}

export interface AssetReportSummary {
  totalAssets: number;
  activeAssets: number;
  archivedAssets: number;
  openIncidents: number;
  activeInventoryCampaigns: number;
  byStatus: ReportCount[];
  byCondition: ReportCount[];
  byBranch: ReportCount[];
  byCategory: ReportCount[];
}
