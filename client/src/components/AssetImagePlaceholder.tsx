interface AssetImagePlaceholderProps {
  assetName: string;
  size?: "thumbnail" | "detail";
}

function getInitials(assetName: string) {
  const words = assetName
    .trim()
    .split(/\s+/)
    .filter(Boolean);

  if (words.length === 0) {
    return "A";
  }

  return words
    .slice(0, 2)
    .map((word) => word[0]?.toUpperCase() ?? "")
    .join("");
}

export function AssetImagePlaceholder({
  assetName,
  size = "thumbnail",
}: AssetImagePlaceholderProps) {
  return (
    <div
      className={`asset-image-placeholder ${size}`}
      aria-label={`Image placeholder for ${assetName}`}
    >
      <span className="asset-image-placeholder-icon">
        {getInitials(assetName)}
      </span>

      {size === "detail" && (
        <span className="asset-image-placeholder-text">
          Image preview will be available when asset image storage is enabled.
        </span>
      )}
    </div>
  );
}
