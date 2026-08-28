import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import { useAssetMasterData } from "../hooks/useAssetMasterData";
import { aimsApi } from "../lib/api";
import type {
  Asset,
  AssetCondition,
  AssetStatus,
  CreateAssetRequest,
} from "../types/aims";

interface AssetRegistrationFormProps {
  onCreated: (asset: Asset) => void;
}

const STATUS_OPTIONS: AssetStatus[] = [
  "Available",
  "Reserved",
  "Issued",
  "ProjectAssigned",
  "InTransit",
  "UnderInspection",
  "UnderMaintenance",
  "UnderCalibration",
  "Quarantined",
  "Missing",
  "LostOrStolen",
  "Retired",
  "Disposed",
];

const CONDITION_OPTIONS: AssetCondition[] = [
  "New",
  "Excellent",
  "Good",
  "Fair",
  "Damaged",
  "Unserviceable",
  "ForDisposal",
];

const initialForm = {
  name: "",
  shortDescription: "",
  categoryId: "",
  serialNumber: "",
  manufacturer: "",
  model: "",
  partNumber: "",
  legacyAssetId: "",
  acquisitionCost: "",
  currency: "PHP",
  companyId: "",
  branchId: "",
  departmentId: "",
  currentLocationId: "",
  status: "Available" as AssetStatus,
  condition: "New" as AssetCondition,
};

type FormState = typeof initialForm;

export function AssetRegistrationForm({
  onCreated,
}: AssetRegistrationFormProps) {
  const [form, setForm] = useState<FormState>(initialForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const masterData = useAssetMasterData(
    form.companyId,
    form.branchId,
  );

  useEffect(() => {
    setForm((current) => ({
      ...current,
      branchId: "",
      departmentId: "",
      currentLocationId: "",
    }));
  }, [form.companyId]);

  useEffect(() => {
    setForm((current) => ({
      ...current,
      departmentId: "",
      currentLocationId: "",
    }));
  }, [form.branchId]);

  const locationOptions = useMemo(
    () =>
      [...masterData.locations].sort((left, right) =>
        left.name.localeCompare(right.name),
      ),
    [masterData.locations],
  );

  const updateField = <K extends keyof FormState>(
    field: K,
    value: FormState[K],
  ) => {
    setForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const validate = () => {
    const required: Array<[string, string]> = [
      [form.name, "Asset name"],
      [form.categoryId, "Category"],
      [form.companyId, "Company"],
      [form.branchId, "Branch"],
      [form.currentLocationId, "Current location"],
    ];

    const missing = required.find(([value]) => !value.trim());

    if (missing) {
      return `${missing[1]} is required.`;
    }

    if (form.currency.trim() && form.currency.trim().length !== 3) {
      return "Currency must be a three-letter ISO code.";
    }

    if (
      form.acquisitionCost.trim() &&
      Number(form.acquisitionCost) < 0
    ) {
      return "Acquisition cost cannot be negative.";
    }

    return null;
  };

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const validationError = validate();

    if (validationError) {
      setError(validationError);
      setSuccess(null);
      return;
    }

    const payload: CreateAssetRequest = {
      name: form.name.trim(),
      shortDescription: form.shortDescription.trim(),

      categoryId: form.categoryId,

      serialNumber: form.serialNumber.trim(),
      manufacturer: form.manufacturer.trim(),
      model: form.model.trim(),
      partNumber: form.partNumber.trim(),
      legacyAssetId: form.legacyAssetId.trim(),

      acquisitionCost: form.acquisitionCost.trim()
        ? Number(form.acquisitionCost)
        : null,
      currency: form.currency.trim().toUpperCase(),

      companyId: form.companyId,
      branchId: form.branchId,
      departmentId: form.departmentId || null,
      currentLocationId: form.currentLocationId,

      currentCustodianId: null,
      barcodeValue: null,

      status: form.status,
      condition: form.condition,
    };

    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      const created = await aimsApi.createAsset(payload);

      setSuccess(`${created.assetId} registered successfully.`);

      setForm((current) => ({
        ...initialForm,
        companyId: current.companyId,
        branchId: current.branchId,
        departmentId: current.departmentId,
        currentLocationId: current.currentLocationId,
        categoryId: current.categoryId,
      }));

      onCreated(created);
    } catch (submitError) {
      setError(
        submitError instanceof Error
          ? submitError.message
          : "Unable to register asset.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="panel">
      <div className="panel-heading">
        <p className="eyebrow">New Asset</p>
        <h2>Register an asset</h2>
        <p className="muted">
          Asset ID and default barcode are assigned automatically.
        </p>
      </div>

      {masterData.error && (
        <div className="alert error">{masterData.error}</div>
      )}
      {error && <div className="alert error">{error}</div>}
      {success && <div className="alert success">{success}</div>}

      <form onSubmit={handleSubmit}>
        <fieldset disabled={saving || masterData.loading}>
          <div className="form-section">
            <div className="section-title">
              <h3>Asset identity</h3>
              <p>Core identifying information for the equipment.</p>
            </div>

            <div className="form-grid">
              <label className="field field-wide">
                <span>Asset name *</span>
                <input
                  value={form.name}
                  onChange={(event) =>
                    updateField("name", event.target.value)
                  }
                  placeholder="e.g. Digital Multimeter"
                />
              </label>

              <label className="field">
                <span>Category *</span>
                <select
                  value={form.categoryId}
                  onChange={(event) =>
                    updateField("categoryId", event.target.value)
                  }
                >
                  <option value="">Select category</option>
                  {masterData.categories.map((category) => (
                    <option key={category.id} value={category.id}>
                      {category.parentCategoryName
                        ? `${category.parentCategoryName} / ${category.name}`
                        : category.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field">
                <span>Serial number</span>
                <input
                  value={form.serialNumber}
                  onChange={(event) =>
                    updateField("serialNumber", event.target.value)
                  }
                  placeholder="Manufacturer serial number"
                />
              </label>

              <label className="field">
                <span>Manufacturer</span>
                <input
                  value={form.manufacturer}
                  onChange={(event) =>
                    updateField("manufacturer", event.target.value)
                  }
                  placeholder="e.g. Fluke"
                />
              </label>

              <label className="field">
                <span>Model</span>
                <input
                  value={form.model}
                  onChange={(event) =>
                    updateField("model", event.target.value)
                  }
                  placeholder="e.g. 87V"
                />
              </label>

              <label className="field">
                <span>Part number</span>
                <input
                  value={form.partNumber}
                  onChange={(event) =>
                    updateField("partNumber", event.target.value)
                  }
                />
              </label>

              <label className="field">
                <span>Legacy asset ID</span>
                <input
                  value={form.legacyAssetId}
                  onChange={(event) =>
                    updateField("legacyAssetId", event.target.value)
                  }
                />
              </label>

              <label className="field field-wide">
                <span>Description</span>
                <textarea
                  rows={3}
                  value={form.shortDescription}
                  onChange={(event) =>
                    updateField(
                      "shortDescription",
                      event.target.value,
                    )
                  }
                  placeholder="Short description or identifying notes"
                />
              </label>
            </div>
          </div>

          <div className="form-section">
            <div className="section-title">
              <h3>Organization & location</h3>
              <p>
                Selections are restricted to active master-data
                relationships.
              </p>
            </div>

            <div className="form-grid">
              <label className="field">
                <span>Company *</span>
                <select
                  value={form.companyId}
                  onChange={(event) =>
                    updateField("companyId", event.target.value)
                  }
                >
                  <option value="">Select company</option>
                  {masterData.companies.map((company) => (
                    <option key={company.id} value={company.id}>
                      {company.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field">
                <span>Branch *</span>
                <select
                  value={form.branchId}
                  disabled={!form.companyId}
                  onChange={(event) =>
                    updateField("branchId", event.target.value)
                  }
                >
                  <option value="">Select branch</option>
                  {masterData.branches.map((branch) => (
                    <option key={branch.id} value={branch.id}>
                      {branch.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field">
                <span>Department</span>
                <select
                  value={form.departmentId}
                  disabled={!form.branchId}
                  onChange={(event) =>
                    updateField("departmentId", event.target.value)
                  }
                >
                  <option value="">No department</option>
                  {masterData.departments.map((department) => (
                    <option
                      key={department.id}
                      value={department.id}
                    >
                      {department.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field">
                <span>Current location *</span>
                <select
                  value={form.currentLocationId}
                  disabled={!form.branchId}
                  onChange={(event) =>
                    updateField(
                      "currentLocationId",
                      event.target.value,
                    )
                  }
                >
                  <option value="">Select location</option>
                  {locationOptions.map((location) => (
                    <option key={location.id} value={location.id}>
                      {location.parentLocationName
                        ? `${location.parentLocationName} / ${location.name}`
                        : location.name}
                    </option>
                  ))}
                </select>
              </label>
            </div>
          </div>

          <div className="form-section">
            <div className="section-title">
              <h3>Financial & lifecycle</h3>
              <p>Initial valuation, status, and physical condition.</p>
            </div>

            <div className="form-grid">
              <label className="field">
                <span>Acquisition cost</span>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.acquisitionCost}
                  onChange={(event) =>
                    updateField(
                      "acquisitionCost",
                      event.target.value,
                    )
                  }
                  placeholder="0.00"
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
                  placeholder="PHP"
                />
              </label>

              <label className="field">
                <span>Status</span>
                <select
                  value={form.status}
                  onChange={(event) =>
                    updateField(
                      "status",
                      event.target.value as AssetStatus,
                    )
                  }
                >
                  {STATUS_OPTIONS.map((status) => (
                    <option key={status} value={status}>
                      {status}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field">
                <span>Condition</span>
                <select
                  value={form.condition}
                  onChange={(event) =>
                    updateField(
                      "condition",
                      event.target.value as AssetCondition,
                    )
                  }
                >
                  {CONDITION_OPTIONS.map((condition) => (
                    <option key={condition} value={condition}>
                      {condition}
                    </option>
                  ))}
                </select>
              </label>
            </div>
          </div>

          <div className="form-actions">
            <button
              type="button"
              className="button secondary"
              onClick={() => {
                setForm(initialForm);
                setError(null);
                setSuccess(null);
              }}
            >
              Clear form
            </button>

            <button
              type="submit"
              className="button primary"
              disabled={saving}
            >
              {saving ? "Registering…" : "Register asset"}
            </button>
          </div>
        </fieldset>
      </form>
    </section>
  );
}
