import {
  useEffect,
  useState,
  type FormEvent,
} from "react";
import { formatAssetEnum } from "../constants/assets";
import { aimsApi } from "../lib/api";
import { getStoredRole, hasCapability } from "../lib/permissions";
import type { Asset } from "../types/aims";
import type {
  AssetIncident,
  AssetIncidentSeverity,
  AssetIncidentType,
} from "../types/v0.9";

interface Props {
  asset: Asset;
}

const TYPES: AssetIncidentType[] = [
  "Damage",
  "Missing",
  "LostOrStolen",
  "Other",
];

const SEVERITIES: AssetIncidentSeverity[] = [
  "Low",
  "Medium",
  "High",
  "Critical",
];

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export function AssetIncidentPanel({ asset }: Props) {
  const canReportIncident =
    hasCapability(
      getStoredRole(),
      "reportIncidents",
    );

  const [incidents, setIncidents] =
    useState<AssetIncident[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [type, setType] =
    useState<AssetIncidentType>("Damage");
  const [severity, setSeverity] =
    useState<AssetIncidentSeverity>("Medium");
  const [description, setDescription] = useState("");
  const [occurredAt, setOccurredAt] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] =
    useState<string | null>(null);

  async function load() {
    try {
      setIncidents(
        await aimsApi.getIncidents(false, asset.id),
      );
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load incidents.",
      );
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [asset.id]);

  async function submit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!description.trim()) {
      setError("Incident description is required.");
      return;
    }

    setSaving(true);

    try {
      await aimsApi.createIncident({
        assetId: asset.id,
        type,
        severity,
        description: description.trim(),
        occurredAt: occurredAt
          ? new Date(occurredAt).toISOString()
          : null,
      });

      setDescription("");
      setOccurredAt("");
      setShowForm(false);
      await load();
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to report incident.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="drawer-section">
      <div className="drawer-section-heading">
        <div>
          <h3>Incidents</h3>
          <p>
            Record damage, missing assets, loss/theft, and other
            asset incidents without overwriting the asset record.
          </p>
        </div>

        {!asset.isArchived && canReportIncident && (
          <button
            type="button"
            className="button secondary compact"
            onClick={() => setShowForm((value) => !value)}
          >
            {showForm ? "Cancel" : "Report incident"}
          </button>
        )}
      </div>

      {error && <div className="alert error">{error}</div>}

      {showForm && (
        <form className="v09-form-card" onSubmit={submit}>
          <div className="v09-form-grid">
            <label className="field">
              <span>Incident type *</span>
              <select
                value={type}
                onChange={(event) =>
                  setType(
                    event.target.value as AssetIncidentType,
                  )
                }
              >
                {TYPES.map((value) => (
                  <option key={value} value={value}>
                    {formatAssetEnum(value)}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Severity *</span>
              <select
                value={severity}
                onChange={(event) =>
                  setSeverity(
                    event.target
                      .value as AssetIncidentSeverity,
                  )
                }
              >
                {SEVERITIES.map((value) => (
                  <option key={value} value={value}>
                    {value}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Occurred at</span>
              <input
                type="datetime-local"
                value={occurredAt}
                onChange={(event) =>
                  setOccurredAt(event.target.value)
                }
              />
            </label>

            <label className="field v09-span-two">
              <span>Description *</span>
              <textarea
                rows={3}
                value={description}
                onChange={(event) =>
                  setDescription(event.target.value)
                }
              />
            </label>
          </div>

          <div className="filter-actions">
            <button
              type="submit"
              className="button primary"
              disabled={saving}
            >
              Report incident
            </button>
          </div>
        </form>
      )}

      <div className="service-history-list">
        {incidents.length === 0 ? (
          <div className="history-empty">
            No incidents recorded.
          </div>
        ) : (
          incidents.map((incident) => (
            <article
              key={incident.id}
              className="service-history-item"
            >
              <div className="service-history-heading">
                <strong>
                  {formatAssetEnum(incident.type)} ·{" "}
                  {incident.severity}
                </strong>
                <span>{incident.status}</span>
              </div>

              <p>{incident.description}</p>
              <p>Occurred: {formatDate(incident.occurredAt)}</p>

              {incident.resolutionNotes && (
                <p>
                  Resolution: {incident.resolutionNotes}
                </p>
              )}
            </article>
          ))
        )}
      </div>
    </div>
  );
}
