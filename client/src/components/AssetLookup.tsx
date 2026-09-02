import {
  useRef,
  useState,
  type FormEvent,
} from "react";
import { aimsApi } from "../lib/api";
import type { Asset } from "../types/aims";
import { AssetQrCode } from "./AssetQrCode";

export function AssetLookup() {
  const [value, setValue] = useState("");
  const [asset, setAsset] =
    useState<Asset | null>(null);
  const [loading, setLoading] =
    useState(false);
  const [error, setError] =
    useState<string | null>(null);

  const inputRef =
    useRef<HTMLInputElement>(null);

  async function lookup(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    const barcode = value.trim();

    if (!barcode) {
      setError(
        "Scan or enter an asset barcode.",
      );
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const result =
        await aimsApi.getAssetByBarcode(
          barcode,
        );

      setAsset(result);
      setValue("");
    } catch (lookupError) {
      setAsset(null);
      setError(
        lookupError instanceof Error
          ? lookupError.message
          : "Asset was not found.",
      );
    } finally {
      setLoading(false);
      window.setTimeout(
        () => inputRef.current?.focus(),
        0,
      );
    }
  }

  return (
    <div className="lookup-layout">
      <section className="panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">
              Identification
            </p>
            <h2>Scan / Lookup</h2>
            <p className="muted">
              Scan a QR/barcode or enter the barcode value manually.
            </p>
          </div>
        </div>

        <form
          className="lookup-form"
          onSubmit={lookup}
        >
          <label className="field">
            <span>
              Barcode / Asset ID
            </span>

            <input
              ref={inputRef}
              autoFocus
              value={value}
              onChange={(event) =>
                setValue(event.target.value)
              }
              placeholder="Scan or type AST-000001"
              autoComplete="off"
            />
          </label>

          <button
            type="submit"
            className="button primary"
            disabled={loading}
          >
            {loading
              ? "Looking up…"
              : "Find asset"}
          </button>
        </form>

        <p className="lookup-hint">
          USB/Bluetooth scanners that act as a keyboard can scan directly into
          this field and submit with Enter.
        </p>

        {error && (
          <div className="alert error">
            {error}
          </div>
        )}
      </section>

      {asset && (
        <section className="panel lookup-result">
          <div className="lookup-result-qr">
            <AssetQrCode
              value={asset.barcodeValue}
              size={150}
            />
          </div>

          <div className="lookup-result-details">
            <p className="eyebrow">
              Asset Found
            </p>

            <h2>{asset.assetId}</h2>
            <h3>{asset.name}</h3>

            <dl className="lookup-detail-grid">
              <div>
                <dt>Status</dt>
                <dd>{asset.status}</dd>
              </div>

              <div>
                <dt>Condition</dt>
                <dd>{asset.condition}</dd>
              </div>

              <div>
                <dt>Location</dt>
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
                <dt>Serial No.</dt>
                <dd>
                  {asset.serialNumber ||
                    "—"}
                </dd>
              </div>

              <div>
                <dt>Barcode</dt>
                <dd className="mono-value">
                  {asset.barcodeValue}
                </dd>
              </div>
            </dl>
          </div>
        </section>
      )}
    </div>
  );
}
