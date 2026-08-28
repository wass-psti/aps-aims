import {
  useEffect,
  useState,
  type FormEvent,
} from "react";
import { aimsApi } from "../lib/api";
import type {
  Asset,
  AssetCustodyHistory,
  AssetLocation,
  Employee,
} from "../types/aims";

interface AssetCustodyPanelProps {
  asset: Asset;
  onAssetChanged: (asset: Asset) => void;
}

type ActionMode = "issue" | "return" | null;

function formatDate(value?: string | null) {
  if (!value) return "—";

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export function AssetCustodyPanel({
  asset,
  onAssetChanged,
}: AssetCustodyPanelProps) {
  const [history, setHistory] = useState<AssetCustodyHistory[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [locations, setLocations] = useState<AssetLocation[]>([]);
  const [mode, setMode] = useState<ActionMode>(null);
  const [employeeId, setEmployeeId] = useState("");
  const [locationId, setLocationId] = useState(asset.currentLocationId);
  const [notes, setNotes] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function loadData() {
    setLoading(true);

    try {
      const [historyResult, employeesResult, locationsResult] =
        await Promise.all([
          aimsApi.getCustodyHistory(asset.id),
          aimsApi.getEmployees(),
          aimsApi.getLocationsByBranch(asset.branchId),
        ]);

      setHistory(historyResult);
      setEmployees(employeesResult.filter((employee) => employee.isActive));
      setLocations(locationsResult.filter((location) => location.isActive));
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load custody information.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
    setMode(null);
    setEmployeeId("");
    setLocationId(asset.currentLocationId);
    setNotes("");
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [asset.id, asset.currentCustodianId, asset.currentLocationId]);

  async function submitAction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (mode === "issue" && !employeeId) {
      setError("Select an employee to issue the asset to.");
      return;
    }

    if (mode === "return" && !locationId) {
      setError("Select a return location.");
      return;
    }

    setSaving(true);
    setError(null);

    try {
      const updated =
        mode === "issue"
          ? await aimsApi.issueAsset(asset.id, {
              employeeId,
              notes: notes.trim(),
            })
          : await aimsApi.returnAsset(asset.id, {
              locationId,
              notes: notes.trim(),
            });

      onAssetChanged(updated);
      setMode(null);
      setEmployeeId("");
      setLocationId(updated.currentLocationId);
      setNotes("");

      const updatedHistory = await aimsApi.getCustodyHistory(asset.id);
      setHistory(updatedHistory);
    } catch (actionError) {
      setError(
        actionError instanceof Error
          ? actionError.message
          : "Unable to complete custody action.",
      );
    } finally {
      setSaving(false);
    }
  }

  const canIssue =
    asset.status === "Available" && !asset.currentCustodianId;

  const canReturn =
    asset.status === "Issued" && Boolean(asset.currentCustodianId);

  return (
    <div className="drawer-section">
      <div className="drawer-section-heading">
        <div>
          <h3>Custody</h3>
          <p>
            Issue and return actions update current state and preserve custody
            history.
          </p>
        </div>

        <div className="custody-actions">
          {canIssue && (
            <button
              type="button"
              className="button primary compact"
              onClick={() => setMode("issue")}
            >
              Issue asset
            </button>
          )}

          {canReturn && (
            <button
              type="button"
              className="button primary compact"
              onClick={() => setMode("return")}
            >
              Return asset
            </button>
          )}
        </div>
      </div>

      {error && <div className="alert error custody-alert">{error}</div>}

      {mode && (
        <form className="custody-form" onSubmit={submitAction}>
          <fieldset disabled={saving}>
            {mode === "issue" ? (
              <label className="field">
                <span>Issue to employee *</span>
                <select
                  value={employeeId}
                  onChange={(event) => setEmployeeId(event.target.value)}
                >
                  <option value="">Select employee</option>
                  {employees.map((employee) => (
                    <option key={employee.id} value={employee.id}>
                      {employee.employeeNumber
                        ? `${employee.displayName} (${employee.employeeNumber})`
                        : employee.displayName}
                    </option>
                  ))}
                </select>
              </label>
            ) : (
              <label className="field">
                <span>Return location *</span>
                <select
                  value={locationId}
                  onChange={(event) => setLocationId(event.target.value)}
                >
                  {locations.map((location) => (
                    <option key={location.id} value={location.id}>
                      {location.parentLocationName
                        ? `${location.parentLocationName} / ${location.name}`
                        : location.name}
                    </option>
                  ))}
                </select>
              </label>
            )}

            <label className="field">
              <span>Notes</span>
              <textarea
                rows={3}
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
                placeholder={
                  mode === "issue"
                    ? "Purpose, expected use, or other issue notes"
                    : "Return condition or handover notes"
                }
              />
            </label>

            <div className="drawer-edit-actions">
              <button
                type="button"
                className="button secondary"
                onClick={() => {
                  setMode(null);
                  setError(null);
                }}
              >
                Cancel
              </button>

              <button type="submit" className="button primary">
                {saving
                  ? "Saving…"
                  : mode === "issue"
                    ? "Confirm issue"
                    : "Confirm return"}
              </button>
            </div>
          </fieldset>
        </form>
      )}

      {loading ? (
        <div className="history-empty">Loading custody history…</div>
      ) : history.length === 0 ? (
        <div className="history-empty">No custody history yet.</div>
      ) : (
        <div className="history-list">
          {history.map((record) => (
            <article className="history-item" key={record.id}>
              <div className="history-item-heading">
                <strong>{record.employeeName}</strong>
                <span className={record.isOpen ? "history-open" : ""}>
                  {record.isOpen ? "Issued" : "Returned"}
                </span>
              </div>

              <dl className="history-meta">
                <div>
                  <dt>Issued</dt>
                  <dd>{formatDate(record.issuedAt)}</dd>
                </div>
                <div>
                  <dt>From</dt>
                  <dd>{record.issuedFromLocationName}</dd>
                </div>
                <div>
                  <dt>Returned</dt>
                  <dd>{formatDate(record.returnedAt)}</dd>
                </div>
                <div>
                  <dt>To</dt>
                  <dd>{record.returnedToLocationName ?? "—"}</dd>
                </div>
              </dl>

              {(record.issueNotes || record.returnNotes) && (
                <div className="history-notes">
                  {record.issueNotes && <p><strong>Issue:</strong> {record.issueNotes}</p>}
                  {record.returnNotes && <p><strong>Return:</strong> {record.returnNotes}</p>}
                </div>
              )}
            </article>
          ))}
        </div>
      )}
    </div>
  );
}
