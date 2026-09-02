export type AimsRole =
  | "Administrator"
  | "AssetManager"
  | "Custodian"
  | "Viewer";

export interface AuthenticatedUser {
  id: string;
  email: string;
  displayName: string;
  role: AimsRole;
}

export interface LoginResponse {
  accessToken: string;
  user: AuthenticatedUser;
  expiresAt: string;
}

export interface ApplicationUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  role: AimsRole;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string | null;
}
