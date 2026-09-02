import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import {
  ASSET_CONDITION_OPTIONS,
  formatAssetEnum,
} from "../constants/assets";
import { aimsApi } from "../lib/api";
import type {
  AssetCondition,
  AssetLocation,
  Branch,
  Company,
} from "../types/aims";
import type { AimsRole } from "../types/auth";
import { hasCapability } from "../lib/permissions";
import type {
  InventoryCampaign,
  InventoryCount,
} from "../types/v0.9";

function formatDate(value?: string | null) {
  return value
    ? new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "short",
      }).format(new Date(value))
    : "—";
}

interface Props {
  role: AimsRole;
}

export function InventoryCampaignsPage({ role }: Props) {
  const canManageCampaign =
    hasCapability(
      role,
      "manageInventory",
    );

  const canCount =
    hasCapability(
      role,
      "countInventory",
    );
  const [campaigns, setCampaigns] =
    useState<InventoryCampaign[]>([]);
  const [selectedId, setSelectedId] =
    useState("");
  const [counts, setCounts] =
    useState<InventoryCount[]>([]);
  const [companies, setCompanies] =
    useState<Company[]>([]);
  const [branches, setBranches] =
    useState<Branch[]>([]);
  const [locations, setLocations] =
    useState<AssetLocation[]>([]);

  const [companyId, setCompanyId] = useState("");
  const [branchId, setBranchId] = useState("");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");

  const [barcode, setBarcode] = useState("");
  const [observedLocationId, setObservedLocationId] =
    useState("");
  const [observedCondition, setObservedCondition] =
    useState<AssetCondition>("Good");
  const [countNotes, setCountNotes] = useState("");

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] =
    useState<string | null>(null);

  const selectedCampaign = useMemo(
    () =>
      campaigns.find(
        (campaign) => campaign.id === selectedId,
      ) ?? null,
    [campaigns, selectedId],
  );

  async function loadCampaigns(preferredId?: string) {
    const data = await aimsApi.getInventoryCampaigns();
    setCampaigns(data);

    const nextId =
      preferredId &&
      data.some((item) => item.id === preferredId)
        ? preferredId
        : selectedId &&
            data.some((item) => item.id === selectedId)
          ? selectedId
          : data[0]?.id ?? "";

    setSelectedId(nextId);
  }

  useEffect(() => {
    async function load() {
      setLoading(true);

      try {
        const [campaignData, companyData] =
          await Promise.all([
            aimsApi.getInventoryCampaigns(),
            aimsApi.getCompanies(),
          ]);

        setCampaigns(campaignData);
        setCompanies(companyData);
        setSelectedId(campaignData[0]?.id ?? "");
        setError(null);
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load inventory campaigns.",
        );
      } finally {
        setLoading(false);
      }
    }

    load();
  }, []);

  useEffect(() => {
    if (!companyId) {
      setBranches([]);
      setBranchId("");
      return;
    }

    aimsApi
      .getBranchesByCompany(companyId)
      .then(setBranches)
      .catch((loadError) =>
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load branches.",
        ),
      );
  }, [companyId]);

  useEffect(() => {
    if (!selectedCampaign) {
      setCounts([]);
      setLocations([]);
      setObservedLocationId("");
      return;
    }

    Promise.all([
      aimsApi.getInventoryCounts(selectedCampaign.id),
      aimsApi.getLocationsByBranch(
        selectedCampaign.branchId,
      ),
    ])
      .then(([countData, locationData]) => {
        setCounts(countData);
        setLocations(locationData);
        setObservedLocationId((current) =>
          locationData.some(
            (location) => location.id === current,
          )
            ? current
            : locationData[0]?.id ?? "",
        );
        setError(null);
      })
      .catch((loadError) =>
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load campaign details.",
        ),
      );
  }, [selectedCampaign?.id, selectedCampaign?.branchId]);

  async function createCampaign(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!name.trim() || !branchId) {
      setError("Campaign name and branch are required.");
      return;
    }

    setSaving(true);

    try {
      const created =
        await aimsApi.createInventoryCampaign({
          name: name.trim(),
          description: description.trim(),
          branchId,
        });

      setName("");
      setDescription("");
      await loadCampaigns(created.id);
      setError(null);
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to create inventory campaign.",
      );
    } finally {
      setSaving(false);
    }
  }

  async function changeCampaignStatus(
    action: "start" | "complete",
  ) {
    if (!selectedCampaign) return;

    setSaving(true);

    try {
      const updated =
        action === "start"
          ? await aimsApi.startInventoryCampaign(
              selectedCampaign.id,
            )
          : await aimsApi.completeInventoryCampaign(
              selectedCampaign.id,
            );

      await loadCampaigns(updated.id);
      setError(null);
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to update inventory campaign.",
      );
    } finally {
      setSaving(false);
    }
  }

  async function recordCount(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (
      !selectedCampaign ||
      !barcode.trim() ||
      !observedLocationId
    ) {
      setError(
        "Barcode and observed location are required.",
      );
      return;
    }

    setSaving(true);

    try {
      await aimsApi.recordInventoryCount(
        selectedCampaign.id,
        {
          barcodeValue: barcode.trim(),
          observedLocationId,
          observedCondition,
          notes: countNotes.trim(),
        },
      );

      const [countData] = await Promise.all([
        aimsApi.getInventoryCounts(selectedCampaign.id),
        loadCampaigns(selectedCampaign.id),
      ]);

      setCounts(countData);
      setBarcode("");
      setCountNotes("");
      setError(null);

      window.setTimeout(() => {
        document
          .querySelector<HTMLInputElement>(
            "#inventory-barcode-input",
          )
          ?.focus();
      }, 0);
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to record inventory count.",
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <section className="panel">
        <div className="empty-state">
          Loading inventory campaigns…
        </div>
      </section>
    );
  }

  return (
    <div className="v09-page-grid">
      <section className="panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Physical Inventory</p>
            <h2>Inventory campaigns</h2>
            <p className="muted">
              Create a branch inventory, scan assets, and record
              location or condition mismatches.
            </p>
          </div>
        </div>

        {error && <div className="alert error">{error}</div>}

        {canManageCampaign && (
          <form
            className="v09-form-card"
            onSubmit={createCampaign}
          >
          <h3>New campaign</h3>

          <div className="v09-form-grid">
            <label className="field">
              <span>Name *</span>
              <input
                value={name}
                onChange={(event) =>
                  setName(event.target.value)
                }
                placeholder="September 2026 Cebu Inventory"
              />
            </label>

            <label className="field">
              <span>Company *</span>
              <select
                value={companyId}
                onChange={(event) =>
                  setCompanyId(event.target.value)
                }
              >
                <option value="">Select company</option>
                {companies.map((company) => (
                  <option key={company.id} value={company.id}>
                    {company.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Branch *</span>
              <select
                value={branchId}
                disabled={!companyId}
                onChange={(event) =>
                  setBranchId(event.target.value)
                }
              >
                <option value="">Select branch</option>
                {branches.map((branch) => (
                  <option key={branch.id} value={branch.id}>
                    {branch.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="field v09-span-two">
              <span>Description</span>
              <input
                value={description}
                onChange={(event) =>
                  setDescription(event.target.value)
                }
              />
            </label>
          </div>

          <div className="filter-actions">
            <button
              className="button primary"
              type="submit"
              disabled={saving}
            >
              Create campaign
            </button>
          </div>
          </form>
        )}

        <div className="v09-campaign-list">
          {campaigns.length === 0 ? (
            <div className="empty-state">
              No inventory campaigns yet.
            </div>
          ) : (
            campaigns.map((campaign) => (
              <button
                key={campaign.id}
                type="button"
                className={
                  campaign.id === selectedId
                    ? "v09-campaign-card active"
                    : "v09-campaign-card"
                }
                onClick={() =>
                  setSelectedId(campaign.id)
                }
              >
                <span>
                  <strong>{campaign.name}</strong>
                  <small>{campaign.branchName}</small>
                </span>

                <span>
                  <strong>
                    {formatAssetEnum(campaign.status)}
                  </strong>
                  <small>
                    {campaign.countedAssets} counted
                  </small>
                </span>
              </button>
            ))
          )}
        </div>
      </section>

      <section className="panel">
        {!selectedCampaign ? (
          <div className="empty-state">
            Select or create an inventory campaign.
          </div>
        ) : (
          <>
            <div className="panel-heading">
              <div>
                <p className="eyebrow">
                  {selectedCampaign.branchName}
                </p>
                <h2>{selectedCampaign.name}</h2>
                <p className="muted">
                  Created {formatDate(selectedCampaign.createdAt)}
                </p>
              </div>

              <span className="badge">
                {formatAssetEnum(selectedCampaign.status)}
              </span>
            </div>

            <div className="v09-action-row">
              {canManageCampaign &&
                selectedCampaign.status === "Draft" && (
                <button
                  type="button"
                  className="button primary"
                  disabled={saving}
                  onClick={() =>
                    changeCampaignStatus("start")
                  }
                >
                  Start campaign
                </button>
              )}

              {canManageCampaign &&
                selectedCampaign.status === "InProgress" && (
                <button
                  type="button"
                  className="button secondary"
                  disabled={saving}
                  onClick={() =>
                    changeCampaignStatus("complete")
                  }
                >
                  Complete campaign
                </button>
              )}
            </div>

            {canCount &&
              selectedCampaign.status === "InProgress" && (
              <form
                className="v09-form-card"
                onSubmit={recordCount}
              >
                <h3>Scan / record asset</h3>

                <div className="v09-form-grid">
                  <label className="field">
                    <span>Barcode *</span>
                    <input
                      id="inventory-barcode-input"
                      autoFocus
                      value={barcode}
                      onChange={(event) =>
                        setBarcode(event.target.value)
                      }
                      placeholder="AST-000001"
                    />
                  </label>

                  <label className="field">
                    <span>Observed location *</span>
                    <select
                      value={observedLocationId}
                      onChange={(event) =>
                        setObservedLocationId(
                          event.target.value,
                        )
                      }
                    >
                      <option value="">
                        Select location
                      </option>
                      {locations.map((location) => (
                        <option
                          key={location.id}
                          value={location.id}
                        >
                          {location.name}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label className="field">
                    <span>Observed condition *</span>
                    <select
                      value={observedCondition}
                      onChange={(event) =>
                        setObservedCondition(
                          event.target
                            .value as AssetCondition,
                        )
                      }
                    >
                      {ASSET_CONDITION_OPTIONS.map(
                        (condition) => (
                          <option
                            key={condition}
                            value={condition}
                          >
                            {formatAssetEnum(condition)}
                          </option>
                        ),
                      )}
                    </select>
                  </label>

                  <label className="field">
                    <span>Notes</span>
                    <input
                      value={countNotes}
                      onChange={(event) =>
                        setCountNotes(event.target.value)
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
                    Record count
                  </button>
                </div>
              </form>
            )}

            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Asset</th>
                    <th>System Location</th>
                    <th>Observed Location</th>
                    <th>Condition</th>
                    <th>Result</th>
                    <th>Counted</th>
                  </tr>
                </thead>
                <tbody>
                  {counts.map((count) => (
                    <tr key={count.id}>
                      <td>
                        <strong>
                          {count.assetBusinessId}
                        </strong>
                        <span className="cell-subtitle">
                          {count.assetName}
                        </span>
                      </td>
                      <td>{count.systemLocationName}</td>
                      <td>{count.observedLocationName}</td>
                      <td>
                        {formatAssetEnum(
                          count.observedCondition,
                        )}
                      </td>
                      <td>
                        <span
                          className={
                            count.result === "Matched"
                              ? "badge"
                              : "badge v09-warning-badge"
                          }
                        >
                          {formatAssetEnum(count.result)}
                        </span>
                      </td>
                      <td>{formatDate(count.countedAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>

              {counts.length === 0 && (
                <div className="empty-state">
                  No assets counted yet.
                </div>
              )}
            </div>
          </>
        )}
      </section>
    </div>
  );
}
