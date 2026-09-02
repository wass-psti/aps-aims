import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import { aimsApi } from "../lib/api";
import { getStoredRole, hasCapability } from "../lib/permissions";
import type {
  Asset,
  AssetLocation,
  AssetTransaction,
} from "../types/aims";

interface AssetTransactionsPanelProps {
  asset: Asset;
  onAssetChanged: (asset: Asset) => void;
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

function displayTransition(
  fromValue?: string | null,
  toValue?: string | null,
) {
  const from = fromValue || "—";
  const to = toValue || "—";

  return from === to ? from : `${from} → ${to}`;
}

export function AssetTransactionsPanel({
  asset,
  onAssetChanged,
}: AssetTransactionsPanelProps) {
  const [transactions, setTransactions] =
    useState<AssetTransaction[]>([]);
  const [locations, setLocations] =
    useState<AssetLocation[]>([]);
  const [transferring, setTransferring] =
    useState(false);
  const [locationId, setLocationId] =
    useState("");
  const [notes, setNotes] =
    useState("");
  const [loading, setLoading] =
    useState(true);
  const [saving, setSaving] =
    useState(false);
  const [error, setError] =
    useState<string | null>(null);

  async function loadData() {
    setLoading(true);

    try {
      const [history, branchLocations] =
        await Promise.all([
          aimsApi.getTransactions(asset.id),
          aimsApi.getLocationsByBranch(asset.branchId),
        ]);

      setTransactions(history);
      setLocations(
        branchLocations.filter(
          (location) => location.isActive,
        ),
      );
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load transaction history.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
    setTransferring(false);
    setLocationId("");
    setNotes("");
    // Reload whenever an issue, return, transfer, or profile update changes UpdatedAt.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [asset.id, asset.updatedAt]);

  const destinations = useMemo(
    () =>
      locations.filter(
        (location) =>
          location.id !== asset.currentLocationId,
      ),
    [locations, asset.currentLocationId],
  );

  const canTransfer =
    hasCapability(
      getStoredRole(),
      "transferAssets",
    ) &&
    asset.status === "Available" &&
    !asset.currentCustodianId &&
    destinations.length > 0;

  async function submitTransfer(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!locationId) {
      setError("Select a destination location.");
      return;
    }

    setSaving(true);
    setError(null);

    try {
      const updated =
        await aimsApi.transferAsset(
          asset.id,
          {
            locationId,
            notes: notes.trim(),
          },
        );

      setTransferring(false);
      setLocationId("");
      setNotes("");
      onAssetChanged(updated);
    } catch (transferError) {
      setError(
        transferError instanceof Error
          ? transferError.message
          : "Unable to transfer asset.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="drawer-section">
      <div className="drawer-section-heading">
        <div>
          <h3>Transactions</h3>
          <p>
            Review issue, return, and location-transfer history.
          </p>
        </div>

        {canTransfer && !transferring && (
          <button
            type="button"
            className="button secondary compact"
            onClick={() =>
              setTransferring(true)
            }
          >
            Transfer asset
          </button>
        )}
      </div>

      {error && (
        <div className="alert error transaction-alert">
          {error}
        </div>
      )}

      {transferring && (
        <form
          className="transaction-form"
          onSubmit={submitTransfer}
        >
          <fieldset disabled={saving}>
            <label className="field">
              <span>Destination location *</span>
              <select
                value={locationId}
                onChange={(event) =>
                  setLocationId(
                    event.target.value,
                  )
                }
              >
                <option value="">
                  Select destination
                </option>

                {destinations.map(
                  (location) => (
                    <option
                      key={location.id}
                      value={location.id}
                    >
                      {location.parentLocationName
                        ? `${location.parentLocationName} / ${location.name}`
                        : location.name}
                    </option>
                  ),
                )}
              </select>
            </label>

            <label className="field">
              <span>Transfer notes</span>
              <textarea
                rows={3}
                value={notes}
                onChange={(event) =>
                  setNotes(event.target.value)
                }
                placeholder="Reason or handover notes"
              />
            </label>

            <div className="drawer-edit-actions">
              <button
                type="button"
                className="button secondary"
                onClick={() => {
                  setTransferring(false);
                  setLocationId("");
                  setNotes("");
                  setError(null);
                }}
              >
                Cancel
              </button>

              <button
                type="submit"
                className="button primary"
              >
                {saving
                  ? "Transferring…"
                  : "Confirm transfer"}
              </button>
            </div>
          </fieldset>
        </form>
      )}

      {loading ? (
        <div className="history-empty">
          Loading transaction history…
        </div>
      ) : transactions.length === 0 ? (
        <div className="history-empty">
          No transactions recorded yet.
        </div>
      ) : (
        <div className="transaction-list">
          {transactions.map(
            (transaction) => (
              <article
                className="transaction-item"
                key={transaction.id}
              >
                <div className="transaction-heading">
                  <strong>
                    {transaction.type}
                  </strong>
                  <span>
                    {formatDate(
                      transaction.occurredAt,
                    )}
                  </span>
                </div>

                <dl className="transaction-meta">
                  <div>
                    <dt>Status</dt>
                    <dd>
                      {displayTransition(
                        transaction.fromStatus,
                        transaction.toStatus,
                      )}
                    </dd>
                  </div>

                  <div>
                    <dt>Location</dt>
                    <dd>
                      {displayTransition(
                        transaction.fromLocationName,
                        transaction.toLocationName,
                      )}
                    </dd>
                  </div>

                  <div>
                    <dt>Custodian</dt>
                    <dd>
                      {displayTransition(
                        transaction.fromCustodianName,
                        transaction.toCustodianName,
                      )}
                    </dd>
                  </div>
                </dl>

                {transaction.notes && (
                  <p className="transaction-note">
                    {transaction.notes}
                  </p>
                )}
              </article>
            ),
          )}
        </div>
      )}
    </div>
  );
}
