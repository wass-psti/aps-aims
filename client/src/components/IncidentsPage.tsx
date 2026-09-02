import {
  useEffect,
  useState,
} from "react";
import { formatAssetEnum } from "../constants/assets";
import { aimsApi } from "../lib/api";
import type { AssetIncident } from "../types/v0.9";
import type { AimsRole } from "../types/auth";
import { hasCapability } from "../lib/permissions";

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

interface Props {
  role: AimsRole;
}

export function IncidentsPage({ role }: Props) {
  const canResolve =
    hasCapability(
      role,
      "resolveIncidents",
    );
  const [incidents, setIncidents] =
    useState<AssetIncident[]>([]);
  const [openOnly, setOpenOnly] = useState(true);
  const [resolvingId, setResolvingId] =
    useState<string | null>(null);
  const [resolutionNotes, setResolutionNotes] =
    useState("");
  const [error, setError] =
    useState<string | null>(null);

  async function load() {
    try {
      setIncidents(await aimsApi.getIncidents(openOnly));
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
  }, [openOnly]);

  async function resolve(incidentId: string) {
    if (!resolutionNotes.trim()) {
      setError("Resolution notes are required.");
      return;
    }

    try {
      await aimsApi.resolveIncident(
        incidentId,
        resolutionNotes.trim(),
      );
      setResolutionNotes("");
      setResolvingId(null);
      await load();
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to resolve incident.",
      );
    }
  }

  return (
    <section className="panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">Loss / Damage</p>
          <h2>Incident management</h2>
          <p className="muted">
            Review and resolve asset incidents from one central
            workspace.
          </p>
        </div>

        <label className="v09-inline-toggle">
          <input
            type="checkbox"
            checked={openOnly}
            onChange={(event) =>
              setOpenOnly(event.target.checked)
            }
          />
          Open incidents only
        </label>
      </div>

      {error && <div className="alert error">{error}</div>}

      <div className="v09-incident-list">
        {incidents.length === 0 ? (
          <div className="empty-state">
            No matching incidents.
          </div>
        ) : (
          incidents.map((incident) => (
            <article
              className="v09-incident-card"
              key={incident.id}
            >
              <div>
                <p className="eyebrow">
                  {incident.assetBusinessId}
                </p>
                <h3>{incident.assetName}</h3>
                <p>{incident.description}</p>

                <div className="v09-incident-meta">
                  <span>
                    {formatAssetEnum(incident.type)}
                  </span>
                  <span>{incident.severity}</span>
                  <span>{incident.status}</span>
                  <span>
                    {formatDate(incident.occurredAt)}
                  </span>
                </div>
              </div>

              <div className="v09-incident-actions">
                {incident.status === "Open" &&
                canResolve ? (
                  resolvingId === incident.id ? (
                    <>
                      <textarea
                        rows={3}
                        value={resolutionNotes}
                        placeholder="Resolution notes..."
                        onChange={(event) =>
                          setResolutionNotes(
                            event.target.value,
                          )
                        }
                      />

                      <button
                        type="button"
                        className="button primary"
                        onClick={() => resolve(incident.id)}
                      >
                        Confirm resolution
                      </button>

                      <button
                        type="button"
                        className="button secondary"
                        onClick={() => {
                          setResolvingId(null);
                          setResolutionNotes("");
                        }}
                      >
                        Cancel
                      </button>
                    </>
                  ) : (
                    <button
                      type="button"
                      className="button primary"
                      onClick={() =>
                        setResolvingId(incident.id)
                      }
                    >
                      Resolve
                    </button>
                  )
                ) : (
                  <p className="muted">
                    {incident.resolutionNotes}
                  </p>
                )}
              </div>
            </article>
          ))
        )}
      </div>
    </section>
  );
}
