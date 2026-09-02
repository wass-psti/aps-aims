import { useEffect, useState } from "react";
import QRCode from "qrcode";
import type { Asset } from "../types/aims";
import { AssetQrCode } from "./AssetQrCode";

interface AssetIdentificationPanelProps {
  asset: Asset;
}

export function AssetIdentificationPanel({
  asset,
}: AssetIdentificationPanelProps) {
  const [copied, setCopied] = useState(false);
  const [printQr, setPrintQr] = useState("");

  useEffect(() => {
    QRCode.toDataURL(asset.barcodeValue, {
      width: 280,
      margin: 1,
      errorCorrectionLevel: "M",
    })
      .then(setPrintQr)
      .catch(() => setPrintQr(""));
  }, [asset.barcodeValue]);

  async function copyBarcode() {
    await navigator.clipboard.writeText(
      asset.barcodeValue,
    );

    setCopied(true);
    window.setTimeout(() => setCopied(false), 1400);
  }

  function printLabel() {
    if (!printQr)
      return;

    const popup = window.open(
      "",
      "_blank",
      "width=520,height=650",
    );

    if (!popup)
      return;

    popup.document.write(`
      <!doctype html>
      <html>
        <head>
          <title>${asset.assetId} Asset Label</title>
          <style>
            body {
              font-family: Arial, sans-serif;
              margin: 0;
              padding: 24px;
              color: #111;
            }
            .label {
              width: 330px;
              border: 1px solid #111;
              padding: 18px;
              text-align: center;
            }
            img {
              display: block;
              width: 220px;
              height: 220px;
              margin: 0 auto 12px;
            }
            .asset-id {
              font-size: 24px;
              font-weight: 800;
              letter-spacing: .03em;
            }
            .name {
              margin-top: 7px;
              font-size: 15px;
              font-weight: 700;
            }
            .barcode {
              margin-top: 12px;
              font-family: monospace;
              font-size: 15px;
            }
            .meta {
              margin-top: 8px;
              color: #444;
              font-size: 12px;
            }
            @media print {
              body { padding: 0; }
            }
          </style>
        </head>
        <body>
          <div class="label">
            <img src="${printQr}" alt="QR code">
            <div class="asset-id">${asset.assetId}</div>
            <div class="name">${asset.name}</div>
            <div class="barcode">${asset.barcodeValue}</div>
            <div class="meta">${asset.companyName} · ${asset.branchName}</div>
          </div>
          <script>
            window.onload = () => {
              window.print();
            };
          </script>
        </body>
      </html>
    `);

    popup.document.close();
  }

  return (
    <div className="drawer-section">
      <div className="drawer-section-heading">
        <div>
          <h3>Identification</h3>
          <p>
            Stable QR and barcode value for physical asset labels.
          </p>
        </div>
      </div>

      <div className="identification-grid">
        <div className="qr-card">
          <AssetQrCode
            value={asset.barcodeValue}
          />
        </div>

        <div className="identification-details">
          <div>
            <span>Asset ID</span>
            <strong>{asset.assetId}</strong>
          </div>

          <div>
            <span>Barcode value</span>
            <strong className="mono-value">
              {asset.barcodeValue}
            </strong>
          </div>

          <p className="muted">
            The QR code encodes the stable barcode value rather than a
            localhost URL, so the label remains portable when AIMS is packaged
            as a standalone application.
          </p>

          <div className="identification-actions">
            <button
              type="button"
              className="button secondary"
              onClick={copyBarcode}
            >
              {copied ? "Copied" : "Copy barcode"}
            </button>

            <button
              type="button"
              className="button primary"
              onClick={printLabel}
              disabled={!printQr}
            >
              Print label
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
