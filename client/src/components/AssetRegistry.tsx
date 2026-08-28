import { useEffect, useMemo, useState } from "react";
import { aimsApi } from "../lib/api";
import type { Asset } from "../types/aims";

interface AssetRegistryProps {
  refreshToken: number;
}

export function AssetRegistry({ refreshToken }: AssetRegistryProps) {
  const [assets, setAssets] = useState<Asset[]>([]);
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadAssets() {
      setLoading(true);

      try {
        const result = await aimsApi.getAssets();

        if (cancelled) return;

        setAssets(result);
        setError(null);
      } catch (loadError) {
        if (cancelled) return;

        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load assets.",
        );
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
  }, [refreshToken]);

  const filteredAssets = useMemo(() => {
    const normalized = query.trim().toLowerCase();

    if (!normalized) return assets;

    return assets.filter((asset) =>
      [
        asset.assetId,
        asset.name,
        asset.serialNumber,
        asset.manufacturer,
        asset.model,
        asset.categoryName,
        asset.branchName,
        asset.currentLocationName,
      ]
        .filter(Boolean)
        .some((value) =>
          String(value).toLowerCase().includes(normalized),
        ),
    );
  }, [assets, query]);

  return (
    <section className="panel">
      <div className="panel-heading registry-heading">
        <div>
          <p className="eyebrow">Asset Registry</p>
          <h2>Registered assets</h2>
          <p className="muted">
            Search and review active APS AIMS asset records.
          </p>
        </div>

        <div className="registry-count">
          <strong>{filteredAssets.length}</strong>
          <span>shown</span>
        </div>
      </div>

      <div className="toolbar">
        <label className="search-field">
          <span>Search</span>
          <input
            type="search"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Asset ID, serial, model, location..."
          />
        </label>
      </div>

      {error && <div className="alert error">{error}</div>}

      {loading ? (
        <div className="empty-state">Loading asset registry…</div>
      ) : filteredAssets.length === 0 ? (
        <div className="empty-state">No matching assets found.</div>
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
              {filteredAssets.map((asset) => (
                <tr key={asset.id}>
                  <td>
                    <strong className="asset-id">
                      {asset.assetId}
                    </strong>
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
                    <span className="badge">{asset.status}</span>
                  </td>
                  <td>{asset.condition}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
