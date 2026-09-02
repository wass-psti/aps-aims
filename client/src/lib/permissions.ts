import type { AimsRole } from "../types/auth";

export type AimsCapability =
  | "manageAssets"
  | "manageEmployees"
  | "operateCustody"
  | "transferAssets"
  | "manageService"
  | "manageInventory"
  | "countInventory"
  | "reportIncidents"
  | "resolveIncidents"
  | "manageUsers";

const managers: AimsRole[] = [
  "Administrator",
  "AssetManager",
];

const operators: AimsRole[] = [
  "Administrator",
  "AssetManager",
  "Custodian",
];

export function hasCapability(
  role: AimsRole,
  capability: AimsCapability,
) {
  switch (capability) {
    case "manageAssets":
    case "manageEmployees":
    case "manageService":
    case "manageInventory":
    case "resolveIncidents":
      return managers.includes(role);

    case "operateCustody":
    case "transferAssets":
    case "countInventory":
    case "reportIncidents":
      return operators.includes(role);

    case "manageUsers":
      return role === "Administrator";

    default:
      return false;
  }
}

export function getStoredRole(): AimsRole {
  const raw =
    localStorage.getItem(
      "aps-aims-auth-user",
    );

  if (!raw) {
    return "Viewer";
  }

  try {
    const parsed = JSON.parse(raw) as {
      role?: AimsRole;
    };

    return parsed.role ?? "Viewer";
  } catch {
    return "Viewer";
  }
}

export function canAccessView(
  role: AimsRole,
  view: string,
) {
  switch (view) {
    case "register":
      return hasCapability(
        role,
        "manageAssets",
      );

    case "employees":
      return hasCapability(
        role,
        "manageEmployees",
      );

    case "users":
    case "audit":
      return hasCapability(
        role,
        "manageUsers",
      );

    case "registry":
    case "identify":
    case "inventory":
    case "incidents":
    case "reports":
      return true;

    default:
      return false;
  }
}
