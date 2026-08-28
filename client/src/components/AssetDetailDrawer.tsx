import {
  useEffect,
  useState,
  type FormEvent,
} from "react";
import { useAssetMasterData } from "../hooks/useAssetMasterData";
import { aimsApi } from "../lib/api";
import type {
  Asset,
  UpdateAssetRequest,
} from "../types/aims";

interface AssetDetailDrawerProps {
  assetId: string | null;
  onClose: () => void;
  onUpdated: (asset: Asset) => void;
}

interface EditState {
  name: string;
  shortDescription: string;
  categoryId: string;
  serialNumber: string;
  manufacturer: string;
  model: string;
  partNumber: string;
  legacyAssetId: string;
  acquisitionCost: string;
  currency: string;
}

function toEditState(asset: Asset): EditState {
  return {
    name: asset.name,
    shortDescription:
      asset.shortDescription ?? "",
    categoryId: asset.categoryId,
    serialNumber: asset.serialNumber ?? "",
    manufacturer: asset.manufacturer ?? "",
    model: asset.model ?? "",
    partNumber: asset.partNumber ?? "",
    legacyAssetId:
      asset.legacyAssetId ?? "",
    acquisitionCost:
      asset.acquisitionCost?.toString() ?? "",
    currency: asset.currency ?? "",
  };
}

function formatDate(value?: string | null) {
  if (!value) {
    return "—";
  }

  return new Intl.DateTimeFormat(
    undefined,
    {
      dateStyle: "medium",
      timeStyle: "short",
    },
  ).format(new Date(value));
}

export function AssetDetailDrawer({
  assetId,
  onClose,
  onUpdated,
}: AssetDetailDrawerProps) {
  const [asset, setAsset] =
    useState<Asset | null>(null);

  const [form, setForm] =
    useState<EditState | null>(null);

  const [editing, setEditing] =
    useState(false);

  const [loading, setLoading] =
    useState(false);

  const [saving, setSaving] =
    useState(false);

  const [error, setError] =
    useState<string | null>(null);

  const masterData = useAssetMasterData("", "");

  useEffect(() => {
    if (!assetId) {
      setAsset(null);
      setForm(null);
      setEditing(false);
      setError(null);
      return;
    }

    let cancelled = false;

    async function loadAsset() {
      setLoading(true);
      setError(null);

      try {
        const result =
          await aimsApi.getAsset(assetId);

        if (cancelled) {
          return;
        }

        setAsset(result);
        setForm(toEditState(result));
      } catch (loadError) {
        if (cancelled) {
          return;
        }

        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load asset.",
        );
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadAsset();

    return () => {
      cancelled = true;
    };
  }, [assetId]);

  useEffect(() => {
    function handleKeyDown(
      event: KeyboardEvent,
    ) {
      if (
        event.key === "Escape" &&
        assetId
      ) {
        onClose();
      }
    }

    window.addEventListener(
      "keydown",
      handleKeyDown,
    );

    return () => {
      window.removeEventListener(
        "keydown",
        handleKeyDown,
      );
    };
  }, [assetId, onClose]);

  if (!assetId) {
    return null;
  }

  const updateField = <
    K extends keyof EditState,
  >(
    field: K,
    value: EditState[K],
  ) => {
    setForm((current) =>
      current
        ? {
            ...current,
            [field]: value,
          }
        : current,
    );
  };

  async function handleSave(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!asset || !form) {
      return;
    }

    if (!form.name.trim()) {
      setError("Asset name is required.");
      return;
    }

    if (!form.categoryId) {
      setError("Category is required.");
      return;
    }

    if (
      form.currency.trim() &&
      form.currency.trim().length !== 3
    ) {
      setError(
        "Currency must be a three-letter ISO code.",
      );
      return;
    }

    if (
      form.acquisitionCost.trim() &&
      Number(form.acquisitionCost) < 0
    ) {
      setError(
        "Acquisition cost cannot be negative.",
      );
      return;
    }

    const payload: UpdateAssetRequest = {
      name: form.name.trim(),
      shortDescription:
        form.shortDescription.trim(),
      categoryId: form.categoryId,
      serialNumber:
        form.serialNumber.trim(),
      manufacturer:
        form.manufacturer.trim(),
      model: form.model.trim(),
      partNumber:
        form.partNumber.trim(),
      legacyAssetId:
        form.legacyAssetId.trim(),
      acquisitionCost:
        form.acquisitionCost.trim()
          ? Number(form.acquisitionCost)
          : null,
      currency:
        form.currency.trim().toUpperCase(),
    };

    setSaving(true);
    setError(null);

    try {
      const updated =
        await aimsApi.updateAsset(
          asset.id,
          payload,
        );

      setAsset(updated);
      setForm(toEditState(updated));
      setEditing(false);
      onUpdated(updated);
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to update asset.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <div
      className="drawer-backdrop"
      onMouseDown={(event) => {
        if (
          event.target ===
          event.currentTarget
        ) {
          onClose();
        }
      }}
    >
      <aside
        className="asset-drawer"
        aria-label="Asset details"
      >
        <div className="drawer-header">
          <div>
            <p className="eyebrow">
              Asset Detail
            </p>
            <h2>
              {asset?.assetId ??
                "Loading asset…"}
            </h2>
          </div>

          <button
            type="button"
            className="icon-button"
            onClick={onClose}
            aria-label="Close asset details"
          >
            ×
          </button>
        </div>

        {error && (
          <div className="alert error">
            {error}
          </div>
        )}

        {loading || !asset || !form ? (
          <div className="drawer-loading">
            Loading asset details…
          </div>
        ) : (
          <>
            <div className="drawer-summary">
              <div>
                <span>Asset</span>
                <strong>{asset.name}</strong>
              </div>

              <div>
                <span>Status</span>
                <strong>{asset.status}</strong>
              </div>

              <div>
                <span>Condition</span>
                <strong>
                  {asset.condition}
                </strong>
              </div>
            </div>

            <form onSubmit={handleSave}>
              <fieldset disabled={saving}>
                <div className="drawer-section">
                  <div className="drawer-section-heading">
                    <div>
                      <h3>Asset profile</h3>
                      <p>
                        Identity and descriptive
                        information can be edited here.
                      </p>
                    </div>

                    {!editing && (
                      <button
                        type="button"
                        className="button secondary compact"
                        onClick={() =>
                          setEditing(true)
                        }
                      >
                        Edit profile
                      </button>
                    )}
                  </div>

                  {editing ? (
                    <div className="drawer-form-grid">
                      <label className="field field-wide">
                        <span>Asset name *</span>
                        <input
                          value={form.name}
                          onChange={(event) =>
                            updateField(
                              "name",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <label className="field field-wide">
                        <span>Category *</span>
                        <select
                          value={form.categoryId}
                          onChange={(event) =>
                            updateField(
                              "categoryId",
                              event.target.value,
                            )
                          }
                        >
                          {masterData.categories.map(
                            (category) => (
                              <option
                                key={category.id}
                                value={category.id}
                              >
                                {category.parentCategoryName
                                  ? `${category.parentCategoryName} / ${category.name}`
                                  : category.name}
                              </option>
                            ),
                          )}
                        </select>
                      </label>

                      <label className="field">
                        <span>Serial number</span>
                        <input
                          value={
                            form.serialNumber
                          }
                          onChange={(event) =>
                            updateField(
                              "serialNumber",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <label className="field">
                        <span>Manufacturer</span>
                        <input
                          value={
                            form.manufacturer
                          }
                          onChange={(event) =>
                            updateField(
                              "manufacturer",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <label className="field">
                        <span>Model</span>
                        <input
                          value={form.model}
                          onChange={(event) =>
                            updateField(
                              "model",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <label className="field">
                        <span>Part number</span>
                        <input
                          value={
                            form.partNumber
                          }
                          onChange={(event) =>
                            updateField(
                              "partNumber",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <label className="field">
                        <span>
                          Legacy asset ID
                        </span>
                        <input
                          value={
                            form.legacyAssetId
                          }
                          onChange={(event) =>
                            updateField(
                              "legacyAssetId",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <label className="field">
                        <span>
                          Acquisition cost
                        </span>
                        <input
                          type="number"
                          min="0"
                          step="0.01"
                          value={
                            form.acquisitionCost
                          }
                          onChange={(event) =>
                            updateField(
                              "acquisitionCost",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <label className="field">
                        <span>Currency</span>
                        <input
                          maxLength={3}
                          value={form.currency}
                          onChange={(event) =>
                            updateField(
                              "currency",
                              event.target.value.toUpperCase(),
                            )
                          }
                        />
                      </label>

                      <label className="field field-wide">
                        <span>Description</span>
                        <textarea
                          rows={4}
                          value={
                            form.shortDescription
                          }
                          onChange={(event) =>
                            updateField(
                              "shortDescription",
                              event.target.value,
                            )
                          }
                        />
                      </label>

                      <div className="drawer-edit-actions field-wide">
                        <button
                          type="button"
                          className="button secondary"
                          onClick={() => {
                            setForm(
                              toEditState(asset),
                            );
                            setEditing(false);
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
                            ? "Saving…"
                            : "Save changes"}
                        </button>
                      </div>
                    </div>
                  ) : (
                    <dl className="detail-grid">
                      <div>
                        <dt>Name</dt>
                        <dd>{asset.name}</dd>
                      </div>
                      <div>
                        <dt>Category</dt>
                        <dd>
                          {asset.categoryName}
                        </dd>
                      </div>
                      <div>
                        <dt>Serial number</dt>
                        <dd>
                          {asset.serialNumber ||
                            "—"}
                        </dd>
                      </div>
                      <div>
                        <dt>Manufacturer</dt>
                        <dd>
                          {asset.manufacturer ||
                            "—"}
                        </dd>
                      </div>
                      <div>
                        <dt>Model</dt>
                        <dd>
                          {asset.model || "—"}
                        </dd>
                      </div>
                      <div>
                        <dt>Part number</dt>
                        <dd>
                          {asset.partNumber ||
                            "—"}
                        </dd>
                      </div>
                      <div>
                        <dt>
                          Legacy asset ID
                        </dt>
                        <dd>
                          {asset.legacyAssetId ||
                            "—"}
                        </dd>
                      </div>
                      <div>
                        <dt>
                          Acquisition cost
                        </dt>
                        <dd>
                          {asset.acquisitionCost !=
                          null
                            ? `${asset.currency ?? ""} ${asset.acquisitionCost.toLocaleString()}`
                            : "—"}
                        </dd>
                      </div>
                      <div className="detail-wide">
                        <dt>Description</dt>
                        <dd>
                          {asset.shortDescription ||
                            "—"}
                        </dd>
                      </div>
                    </dl>
                  )}
                </div>

                <div className="drawer-section">
                  <div className="drawer-section-heading">
                    <div>
                      <h3>
                        Assignment &amp;
                        lifecycle
                      </h3>
                      <p>
                        Operational state is
                        read-only until transaction
                        workflows are introduced.
                      </p>
                    </div>
                  </div>

                  <dl className="detail-grid">
                    <div>
                      <dt>Company</dt>
                      <dd>
                        {asset.companyName}
                      </dd>
                    </div>
                    <div>
                      <dt>Branch</dt>
                      <dd>
                        {asset.branchName}
                      </dd>
                    </div>
                    <div>
                      <dt>Department</dt>
                      <dd>
                        {asset.departmentName ||
                          "—"}
                      </dd>
                    </div>
                    <div>
                      <dt>
                        Current location
                      </dt>
                      <dd>
                        {asset.currentLocationName}
                      </dd>
                    </div>
                    <div>
                      <dt>Custodian</dt>
                      <dd>
                        {asset.currentCustodianName ||
                          "—"}
                      </dd>
                    </div>
                    <div>
                      <dt>Barcode</dt>
                      <dd>
                        {asset.barcodeValue}
                      </dd>
                    </div>
                    <div>
                      <dt>Created</dt>
                      <dd>
                        {formatDate(
                          asset.createdAt,
                        )}
                      </dd>
                    </div>
                    <div>
                      <dt>Last updated</dt>
                      <dd>
                        {formatDate(
                          asset.updatedAt,
                        )}
                      </dd>
                    </div>
                  </dl>
                </div>
              </fieldset>
            </form>
          </>
        )}
      </aside>
    </div>
  );
}
