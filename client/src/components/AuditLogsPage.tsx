import {
  useEffect,
  useMemo,
  useState,
} from "react";
import { authApi } from "../lib/api";
import type { AuditLogEntry } from "../types/audit";

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "medium",
  }).format(new Date(value));
}

export function AuditLogsPage() {
  const [logs, setLogs] =
    useState<AuditLogEntry[]>([]);
  const [search, setSearch] =
    useState("");
  const [error, setError] =
    useState<string | null>(null);

  async function load() {
    try {
      setLogs(
        await authApi.getAuditLogs(300),
      );
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load audit logs.",
      );
    }
  }

  useEffect(() => {
    load();
  }, []);

  const filteredLogs = useMemo(() => {
    const term = search
      .trim()
      .toLowerCase();

    if (!term) {
      return logs;
    }

    return logs.filter((log) =>
      [
        log.userDisplayName,
        log.userEmail,
        log.userRole,
        log.action,
        log.resource,
        log.resourceId,
        log.httpMethod,
        log.path,
        String(log.statusCode),
      ]
        .filter(Boolean)
        .some((value) =>
          String(value)
            .toLowerCase()
            .includes(term),
        ),
    );
  }, [logs, search]);

  return (
    <section className="panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">
            Administration
          </p>
          <h2>Audit Logs</h2>
          <p className="muted">
            Immutable history of
            application write operations
            and authorization outcomes.
          </p>
        </div>

        <button
          type="button"
          className="button secondary"
          onClick={load}
        >
          Refresh
        </button>
      </div>

      {error && (
        <div className="alert error">
          {error}
        </div>
      )}

      <div className="audit-toolbar">
        <label className="search-field">
          <span>Search audit history</span>
          <input
            type="search"
            value={search}
            placeholder="User, resource, action, status..."
            onChange={(event) =>
              setSearch(
                event.target.value,
              )
            }
          />
        </label>

        <span className="muted">
          {filteredLogs.length} shown
        </span>
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Time</th>
              <th>User</th>
              <th>Role</th>
              <th>Action</th>
              <th>Resource</th>
              <th>Status</th>
            </tr>
          </thead>

          <tbody>
            {filteredLogs.map((log) => (
              <tr key={log.id}>
                <td>
                  {formatDate(
                    log.occurredAt,
                  )}
                </td>

                <td>
                  <strong>
                    {log.userDisplayName ??
                      "Unauthenticated"}
                  </strong>
                  <span className="cell-subtitle">
                    {log.userEmail ?? "—"}
                  </span>
                </td>

                <td>
                  {log.userRole ?? "—"}
                </td>

                <td>
                  <strong>
                    {log.action}
                  </strong>
                  <span className="cell-subtitle">
                    {log.httpMethod}{" "}
                    {log.path}
                  </span>
                </td>

                <td>
                  <strong>
                    {log.resource}
                  </strong>
                  <span className="cell-subtitle">
                    {log.resourceId ?? "—"}
                  </span>
                </td>

                <td>
                  <span
                    className={
                      log.statusCode >= 400
                        ? "badge audit-failed"
                        : "badge"
                    }
                  >
                    {log.statusCode}
                  </span>
                </td>
              </tr>
            ))}

            {filteredLogs.length === 0 && (
              <tr>
                <td
                  colSpan={6}
                  className="empty-state"
                >
                  No matching audit entries.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
}
