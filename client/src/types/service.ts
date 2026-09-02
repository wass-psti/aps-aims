export type CalibrationResult =
  | "Passed"
  | "Failed"
  | "Conditional";

export interface AssetMaintenance {
  id: string;
  assetId: string;
  description: string;
  serviceProvider?: string | null;
  startNotes?: string | null;
  startedAt: string;
  completedAt?: string | null;
  completionNotes?: string | null;
  cost?: number | null;
  currency?: string | null;
  nextMaintenanceDueAt?: string | null;
  isOpen: boolean;
}

export interface AssetCalibration {
  id: string;
  assetId: string;
  serviceProvider?: string | null;
  startNotes?: string | null;
  startedAt: string;
  completedAt?: string | null;
  certificateNumber?: string | null;
  result?: CalibrationResult | null;
  completionNotes?: string | null;
  nextCalibrationDueAt?: string | null;
  isOpen: boolean;
}

export interface StartMaintenanceRequest {
  description: string;
  serviceProvider: string;
  notes: string;
}

export interface CompleteMaintenanceRequest {
  completionNotes: string;
  cost: number | null;
  currency: string;
  nextMaintenanceDueAt: string | null;
}

export interface StartCalibrationRequest {
  serviceProvider: string;
  notes: string;
}

export interface CompleteCalibrationRequest {
  certificateNumber: string;
  result: CalibrationResult;
  completionNotes: string;
  nextCalibrationDueAt: string | null;
}
