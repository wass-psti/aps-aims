import type {
  AssetCondition,
  AssetStatus,
} from "../types/aims";

export const ASSET_STATUS_OPTIONS: AssetStatus[] = [
  "Available",
  "Reserved",
  "Issued",
  "ProjectAssigned",
  "InTransit",
  "UnderInspection",
  "UnderMaintenance",
  "UnderCalibration",
  "Quarantined",
  "Missing",
  "LostOrStolen",
  "Retired",
  "Disposed",
];

export const ASSET_CONDITION_OPTIONS: AssetCondition[] = [
  "New",
  "Excellent",
  "Good",
  "Fair",
  "Damaged",
  "Unserviceable",
  "ForDisposal",
];

export function formatAssetEnum(value: string) {
  return value.replace(/([a-z0-9])([A-Z])/g, "$1 $2");
}
