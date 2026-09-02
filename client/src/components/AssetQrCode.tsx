import { useEffect, useState } from "react";
import QRCode from "qrcode";

interface AssetQrCodeProps {
  value: string;
  size?: number;
}

export function AssetQrCode({
  value,
  size = 180,
}: AssetQrCodeProps) {
  const [src, setSrc] = useState("");

  useEffect(() => {
    let cancelled = false;

    QRCode.toDataURL(value, {
      width: size,
      margin: 1,
      errorCorrectionLevel: "M",
    })
      .then((dataUrl) => {
        if (!cancelled)
          setSrc(dataUrl);
      })
      .catch(() => {
        if (!cancelled)
          setSrc("");
      });

    return () => {
      cancelled = true;
    };
  }, [value, size]);

  return src ? (
    <img
      src={src}
      width={size}
      height={size}
      className="asset-qr-image"
      alt={`QR code for ${value}`}
    />
  ) : (
    <div className="qr-placeholder">
      Generating QR…
    </div>
  );
}
