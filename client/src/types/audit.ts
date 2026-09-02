export interface AuditLogEntry {
  id: string;
  userId?: string | null;
  userEmail?: string | null;
  userDisplayName?: string | null;
  userRole?: string | null;
  action: string;
  resource: string;
  resourceId?: string | null;
  httpMethod: string;
  path: string;
  statusCode: number;
  ipAddress?: string | null;
  userAgent?: string | null;
  occurredAt: string;
}
