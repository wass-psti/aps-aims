import {
  useEffect,
  useState,
  type FormEvent,
} from "react";
import {
  ASSET_CONDITION_OPTIONS,
  ASSET_STATUS_OPTIONS,
  formatAssetEnum,
} from "../constants/assets";
import { useAssetMasterData } from "../hooks/useAssetMasterData";
import { aimsApi } from "../lib/api";
import type {
  Asset,
  AssetFilters,
} from "../types/aims";
import { AssetDetailDrawer } from "./AssetDetailDrawer";

interface AssetRegistryProps {
  refreshToken: number;
}

const emptyFilters: AssetFilters = {
  search: "",
  categoryId: "",
  companyId: "",
  branchId: "",
  departmentId: "",
  locationId: "",
  status: "",
  condition: "",
};

const SEARCH_DEBOUNCE_MS = 300;

export function AssetRegistry({
  refreshToken,
}: AssetRegistryProps) {
  const [assets, setAssets] = useState<Asset[]>([]);
  const [draftFilters, setDraftFilters] =
    useState<AssetFilters>(emptyFilters);
  const [appliedFilters, setAppliedFilters] =
    useState<AssetFilters>(emptyFilters);
  const [localRefresh, setLocalRefresh] = useState(0);
  const [selectedAssetId, setSelectedAssetId] =
    useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const masterData = useAssetMasterData(
    draftFilters.companyId ?? "",
    draftFilters.branchId ?? "",
  );

  // Live search with debounce: only the search term is auto-applied.
  // Other filters remain controlled by Apply filters.
  useEffect(() => {
    const timer = window.setTimeout(() => {
      const search = draftFilters.search?.trim() ?? "";

      setAppliedFilters((current) =>
        (current.search ?? "") === search
          ? current
          : { ...current, search },
      );
    }, SEARCH_DEBOUNCE_MS);

    return () => window.clearTimeout(timer);
  }, [draftFilters.search]);

  useEffect(() => {
    let cancelled = false;

    async function loadAssets() {
      setLoading(true);

      try {
        const result = await aimsApi.getAssets(appliedFilters);

        if (!cancelled) {
          setAssets(result);
          setError(null);
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : "Unable to load assets.",
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadAssets();

    return () => {
      cancelled = true;
    };
  }, [refreshToken, localRefresh, appliedFilters]);

  const setFilter = <K extends keyof AssetFilters>(
    key: K,
    value: AssetFilters[K],
  ) => {
    setDraftFilters((current) => {
      const next = { ...current, [key]: value };

      if (key === "companyId") {
        next.branchId = "";
        next.departmentId = "";
        next.locationId = "";
      }

      if (key === "branchId") {
        next.departmentId = "";
        next.locationId = "";
      }

      return next;
    });
  };

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setAppliedFilters({
      ...draftFilters,
      search: draftFilters.search?.trim() ?? "",
    });
  }

  function clearFilters() {
    setDraftFilters(emptyFilters);
    setAppliedFilters(emptyFilters);
  }

  return (
    <>
      <section className="panel">
        <div className="panel-heading registry-heading">
          <div>
            <p className="eyebrow">Asset Registry</p>
            <h2>Registered assets</h2>
            <p className="muted">
              Search, filter, review, and edit asset profiles.
            </p>
          </div>

          <div className="registry-count">
            <strong>{assets.length}</strong>
            <span>shown</span>
          </div>
        </div>

        <form className="filter-panel" onSubmit={applyFilters}>
          <label className="search-field filter-search">
            <span>Search</span>
            <input
              type="search"
              value={draftFilters.search ?? ""}
              onChange={(event) =>
                setFilter("search", event.target.value)
              }
              placeholder="Asset ID, serial, model, location..."
            />
          </label>

          <div className="filter-grid">
            <label className="field">
              <span>Category</span>
              <select
                value={draftFilters.categoryId ?? ""}
                onChange={(event) =>
                  setFilter("categoryId", event.target.value)
                }
              >
                <option value="">All categories</option>
                {masterData.categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Company</span>
              <select
                value={draftFilters.companyId ?? ""}
                onChange={(event) =>
                  setFilter("companyId", event.target.value)
                }
              >
                <option value="">All companies</option>
                {masterData.companies.map((company) => (
                  <option key={company.id} value={company.id}>
                    {company.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Branch</span>
              <select
                value={draftFilters.branchId ?? ""}
                disabled={!draftFilters.companyId}
                onChange={(event) =>
                  setFilter("branchId", event.target.value)
                }
              >
                <option value="">All branches</option>
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
                value={draftFilters.departmentId ?? ""}
                disabled={!draftFilters.branchId}
                onChange={(event) =>
                  setFilter("departmentId", event.target.value)
                }
              >
                <option value="">All departments</option>
                {masterData.departments.map((department) => (
                  <option key={department.id} value={department.id}>
                    {department.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Location</span>
              <select
                value={draftFilters.locationId ?? ""}
                disabled={!draftFilters.branchId}
                onChange={(event) =>
                  setFilter("locationId", event.target.value)
                }
              >
                <option value="">All locations</option>
                {masterData.locations.map((location) => (
                  <option key={location.id} value={location.id}>
                    {location.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Status</span>
              <select
                value={draftFilters.status ?? ""}
                onChange={(event) =>
                  setFilter(
                    "status",
                    event.target.value as AssetFilters["status"],
                  )
                }
              >
                <option value="">All statuses</option>
                {ASSET_STATUS_OPTIONS.map((status) => (
                  <option key={status} value={status}>
                    {formatAssetEnum(status)}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Condition</span>
              <select
                value={draftFilters.condition ?? ""}
                onChange={(event) =>
                  setFilter(
                    "condition",
                    event.target.value as AssetFilters["condition"],
                  )
                }
              >
                <option value="">All conditions</option>
                {ASSET_CONDITION_OPTIONS.map((condition) => (
                  <option key={condition} value={condition}>
                    {formatAssetEnum(condition)}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="filter-actions">
            <button
              type="button"
              className="button secondary"
              onClick={clearFilters}
            >
              Clear filters
            </button>

            <button type="submit" className="button primary">
              Apply filters
            </button>
          </div>
        </form>

        {masterData.error && (
          <div className="alert error">{masterData.error}</div>
        )}

        {error && <div className="alert error">{error}</div>}

        {loading ? (
          <div className="empty-state">
            Loading asset registry…
          </div>
        ) : assets.length === 0 ? (
          <div className="empty-state">
            No matching assets found.
          </div>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Asset ID</th>
                  <th>Asset</th>
                  <th>Category</th>
                  <th>Serial No.</th>
                  <th>Location</th>
                  <th>Status</th>
                  <th>Condition</th>
                </tr>
              </thead>

              <tbody>
                {assets.map((asset) => (
                  <tr key={asset.id}>
                    <td>
                      <button
                        type="button"
                        className="asset-link"
                        onClick={() => setSelectedAssetId(asset.id)}
                      >
                        {asset.assetId}
                      </button>
                    </td>

                    <td>
                      <strong>{asset.name}</strong>
                      <span className="cell-subtitle">
                        {[asset.manufacturer, asset.model]
                          .filter(Boolean)
                          .join(" ") || "—"}
                      </span>
                    </td>

                    <td>{asset.categoryName}</td>
                    <td>{asset.serialNumber || "—"}</td>

                    <td>
                      <strong>{asset.currentLocationName}</strong>
                      <span className="cell-subtitle">
                        {asset.branchName}
                      </span>
                    </td>

                    <td>
                      <span className="badge">
                        {formatAssetEnum(asset.status)}
                      </span>
                    </td>

                    <td>{formatAssetEnum(asset.condition)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <AssetDetailDrawer
        assetId={selectedAssetId}
        onClose={() => setSelectedAssetId(null)}
        onUpdated={() =>
          setLocalRefresh((current) => current + 1)
        }
      />
    </>
  );
}
