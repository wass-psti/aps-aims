import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import { aimsApi } from "../lib/api";
import { getStoredRole, hasCapability } from "../lib/permissions";
import type { Asset } from "../types/aims";
import type {
  AssetCalibration,
  AssetMaintenance,
  CalibrationResult,
} from "../types/service";

interface AssetServicePanelProps {
  asset: Asset;
  onAssetChanged: (asset: Asset) => void;
}

type Mode =
  | "maintenance-start"
  | "maintenance-complete"
  | "calibration-start"
  | "calibration-complete"
  | null;

const toIsoDate = (value: string) =>
  value
    ? new Date(`${value}T00:00:00`).toISOString()
    : null;

const formatDate = (
  value?: string | null,
) =>
  value
    ? new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "short",
      }).format(new Date(value))
    : "—";

export function AssetServicePanel({
  asset,
  onAssetChanged,
}: AssetServicePanelProps) {
  const [maintenance, setMaintenance] =
    useState<AssetMaintenance[]>([]);
  const [calibrations, setCalibrations] =
    useState<AssetCalibration[]>([]);
  const [mode, setMode] =
    useState<Mode>(null);
  const [loading, setLoading] =
    useState(true);
  const [saving, setSaving] =
    useState(false);
  const [error, setError] =
    useState<string | null>(null);

  const [description, setDescription] =
    useState("");
  const [provider, setProvider] =
    useState("");
  const [notes, setNotes] =
    useState("");
  const [cost, setCost] =
    useState("");
  const [currency, setCurrency] =
    useState("PHP");
  const [nextDue, setNextDue] =
    useState("");
  const [certificate, setCertificate] =
    useState("");
  const [result, setResult] =
    useState<CalibrationResult>("Passed");

  const openMaintenance = useMemo(
    () =>
      maintenance.find(
        (record) => record.isOpen,
      ) ?? null,
    [maintenance],
  );

  const openCalibration = useMemo(
    () =>
      calibrations.find(
        (record) => record.isOpen,
      ) ?? null,
    [calibrations],
  );

  const canManageService =
    hasCapability(
      getStoredRole(),
      "manageService",
    );

  const canStart =
    canManageService &&
    asset.status === "Available" &&
    !asset.currentCustodianId;

  async function loadData() {
    setLoading(true);

    try {
      const [maintenanceData, calibrationData] =
        await Promise.all([
          aimsApi.getMaintenanceHistory(asset.id),
          aimsApi.getCalibrationHistory(asset.id),
        ]);

      setMaintenance(maintenanceData);
      setCalibrations(calibrationData);
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load maintenance and calibration history.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
    setMode(null);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [asset.id, asset.updatedAt]);

  function resetForm() {
    setMode(null);
    setDescription("");
    setProvider("");
    setNotes("");
    setCost("");
    setCurrency("PHP");
    setNextDue("");
    setCertificate("");
    setResult("Passed");
    setError(null);
  }

  async function submit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (
      mode === "maintenance-start" &&
      !description.trim()
    ) {
      setError(
        "Maintenance description is required.",
      );
      return;
    }

    setSaving(true);
    setError(null);

    try {
      let updated: Asset;

      switch (mode) {
        case "maintenance-start":
          updated =
            await aimsApi.startMaintenance(
              asset.id,
              {
                description:
                  description.trim(),
                serviceProvider:
                  provider.trim(),
                notes: notes.trim(),
              },
            );
          break;

        case "maintenance-complete":
          if (!openMaintenance)
            return;

          updated =
            await aimsApi.completeMaintenance(
              asset.id,
              openMaintenance.id,
              {
                completionNotes:
                  notes.trim(),
                cost: cost.trim()
                  ? Number(cost)
                  : null,
                currency:
                  currency.trim().toUpperCase(),
                nextMaintenanceDueAt:
                  toIsoDate(nextDue),
              },
            );
          break;

        case "calibration-start":
          updated =
            await aimsApi.startCalibration(
              asset.id,
              {
                serviceProvider:
                  provider.trim(),
                notes: notes.trim(),
              },
            );
          break;

        case "calibration-complete":
          if (!openCalibration)
            return;

          updated =
            await aimsApi.completeCalibration(
              asset.id,
              openCalibration.id,
              {
                certificateNumber:
                  certificate.trim(),
                result,
                completionNotes:
                  notes.trim(),
                nextCalibrationDueAt:
                  toIsoDate(nextDue),
              },
            );
          break;

        default:
          return;
      }

      resetForm();
      onAssetChanged(updated);
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to complete service action.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="drawer-section">
      <div className="drawer-section-heading">
        <div>
          <h3>
            Maintenance &amp; Calibration
          </h3>
          <p>
            Service workflows update the current asset status while preserving
            permanent maintenance, calibration, and transaction history.
          </p>
        </div>

        <div className="service-actions">
          {canStart && !mode && (
            <>
              <button
                type="button"
                className="button secondary compact"
                onClick={() =>
                  setMode(
                    "maintenance-start",
                  )
                }
              >
                Start maintenance
              </button>

              <button
                type="button"
                className="button secondary compact"
                onClick={() =>
                  setMode(
                    "calibration-start",
                  )
                }
              >
                Start calibration
              </button>
            </>
          )}

          {canManageService &&
            asset.status ===
              "UnderMaintenance" &&
            openMaintenance &&
            !mode && (
              <button
                type="button"
                className="button primary compact"
                onClick={() =>
                  setMode(
                    "maintenance-complete",
                  )
                }
              >
                Complete maintenance
              </button>
            )}

          {canManageService &&
            asset.status ===
              "UnderCalibration" &&
            openCalibration &&
            !mode && (
              <button
                type="button"
                className="button primary compact"
                onClick={() =>
                  setMode(
                    "calibration-complete",
                  )
                }
              >
                Complete calibration
              </button>
            )}
        </div>
      </div>

      {error && (
        <div className="alert error service-alert">
          {error}
        </div>
      )}

      {mode && (
        <form
          className="service-form"
          onSubmit={submit}
        >
          <fieldset disabled={saving}>
            {mode ===
              "maintenance-start" && (
              <label className="field">
                <span>
                  Maintenance description *
                </span>
                <input
                  value={description}
                  onChange={(event) =>
                    setDescription(
                      event.target.value,
                    )
                  }
                  placeholder="e.g. Preventive maintenance and inspection"
                />
              </label>
            )}

            {(mode ===
              "maintenance-start" ||
              mode ===
                "calibration-start") && (
              <label className="field">
                <span>
                  Service provider
                </span>
                <input
                  value={provider}
                  onChange={(event) =>
                    setProvider(
                      event.target.value,
                    )
                  }
                  placeholder="Internal team or external provider"
                />
              </label>
            )}

            {mode ===
              "maintenance-complete" && (
              <div className="service-form-grid">
                <label className="field">
                  <span>Cost</span>
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    value={cost}
                    onChange={(event) =>
                      setCost(
                        event.target.value,
                      )
                    }
                  />
                </label>

                <label className="field">
                  <span>Currency</span>
                  <input
                    maxLength={3}
                    value={currency}
                    onChange={(event) =>
                      setCurrency(
                        event.target.value.toUpperCase(),
                      )
                    }
                  />
                </label>
              </div>
            )}

            {mode ===
              "calibration-complete" && (
              <div className="service-form-grid">
                <label className="field">
                  <span>
                    Certificate number
                  </span>
                  <input
                    value={certificate}
                    onChange={(event) =>
                      setCertificate(
                        event.target.value,
                      )
                    }
                  />
                </label>

                <label className="field">
                  <span>Result *</span>
                  <select
                    value={result}
                    onChange={(event) =>
                      setResult(
                        event.target
                          .value as CalibrationResult,
                      )
                    }
                  >
                    <option value="Passed">
                      Passed
                    </option>
                    <option value="Failed">
                      Failed
                    </option>
                    <option value="Conditional">
                      Conditional
                    </option>
                  </select>
                </label>
              </div>
            )}

            {(mode ===
              "maintenance-complete" ||
              mode ===
                "calibration-complete") && (
              <label className="field">
                <span>Next due date</span>
                <input
                  type="date"
                  value={nextDue}
                  onChange={(event) =>
                    setNextDue(
                      event.target.value,
                    )
                  }
                />
              </label>
            )}

            <label className="field">
              <span>
                {mode?.endsWith(
                  "complete",
                )
                  ? "Completion notes"
                  : "Start notes"}
              </span>
              <textarea
                rows={3}
                value={notes}
                onChange={(event) =>
                  setNotes(
                    event.target.value,
                  )
                }
              />
            </label>

            <div className="drawer-edit-actions">
              <button
                type="button"
                className="button secondary"
                onClick={resetForm}
              >
                Cancel
              </button>

              <button
                type="submit"
                className="button primary"
              >
                {saving
                  ? "Saving…"
                  : "Confirm"}
              </button>
            </div>
          </fieldset>
        </form>
      )}

      {loading ? (
        <div className="history-empty">
          Loading service history…
        </div>
      ) : (
        <div className="service-history-grid">
          <section>
            <h4>Maintenance history</h4>

            {maintenance.length === 0 ? (
              <div className="history-empty">
                No maintenance records.
              </div>
            ) : (
              <div className="service-history-list">
                {maintenance.map(
                  (record) => (
                    <article
                      key={record.id}
                      className="service-history-item"
                    >
                      <div className="service-history-heading">
                        <strong>
                          {record.description}
                        </strong>
                        <span>
                          {record.isOpen
                            ? "OPEN"
                            : "COMPLETED"}
                        </span>
                      </div>

                      <p>
                        Started:{" "}
                        {formatDate(
                          record.startedAt,
                        )}
                      </p>

                      <p>
                        Completed:{" "}
                        {formatDate(
                          record.completedAt,
                        )}
                      </p>

                      {record.serviceProvider && (
                        <p>
                          Provider:{" "}
                          {
                            record.serviceProvider
                          }
                        </p>
                      )}

                      {record.cost != null && (
                        <p>
                          Cost:{" "}
                          {record.currency ??
                            ""}{" "}
                          {record.cost.toLocaleString()}
                        </p>
                      )}

                      {record.nextMaintenanceDueAt && (
                        <p>
                          Next due:{" "}
                          {formatDate(
                            record.nextMaintenanceDueAt,
                          )}
                        </p>
                      )}
                    </article>
                  ),
                )}
              </div>
            )}
          </section>

          <section>
            <h4>Calibration history</h4>

            {calibrations.length === 0 ? (
              <div className="history-empty">
                No calibration records.
              </div>
            ) : (
              <div className="service-history-list">
                {calibrations.map(
                  (record) => (
                    <article
                      key={record.id}
                      className="service-history-item"
                    >
                      <div className="service-history-heading">
                        <strong>
                          Calibration
                        </strong>
                        <span>
                          {record.isOpen
                            ? "OPEN"
                            : record.result ??
                              "COMPLETED"}
                        </span>
                      </div>

                      <p>
                        Started:{" "}
                        {formatDate(
                          record.startedAt,
                        )}
                      </p>

                      <p>
                        Completed:{" "}
                        {formatDate(
                          record.completedAt,
                        )}
                      </p>

                      {record.serviceProvider && (
                        <p>
                          Provider:{" "}
                          {
                            record.serviceProvider
                          }
                        </p>
                      )}

                      {record.certificateNumber && (
                        <p>
                          Certificate:{" "}
                          {
                            record.certificateNumber
                          }
                        </p>
                      )}

                      {record.nextCalibrationDueAt && (
                        <p>
                          Next due:{" "}
                          {formatDate(
                            record.nextCalibrationDueAt,
                          )}
                        </p>
                      )}
                    </article>
                  ),
                )}
              </div>
            )}
          </section>
        </div>
      )}
    </div>
  );
}
