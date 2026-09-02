import {
  useEffect,
  useState,
} from "react";
import { formatAssetEnum } from "../constants/assets";
import { aimsApi } from "../lib/api";
import type {
  AssetReportSummary,
  ReportCount,
} from "../types/v0.9";

function escapeCsv(value: string | number) {
  const text = String(value);
  return `"${text.replaceAll('"', '""')}"`;
}

function rowsFor(
  heading: string,
  rows: ReportCount[],
) {
  return [
    [heading, "Count"],
    ...rows.map((row) => [
      formatAssetEnum(row.label),
      String(row.count),
    ]),
  ];
}

export function ReportsPage() {
  const [summary, setSummary] =
    useState<AssetReportSummary | null>(null);
  const [error, setError] =
    useState<string | null>(null);

  async function load() {
    try {
      setSummary(await aimsApi.getReportSummary());
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load reports.",
      );
    }
  }

  useEffect(() => {
    load();
  }, []);

  function exportCsv() {
    if (!summary) return;

    const blocks = [
      [
        ["APS AIMS Asset Summary", "Value"],
        ["Total Assets", String(summary.totalAssets)],
        ["Active Assets", String(summary.activeAssets)],
        ["Archived Assets", String(summary.archivedAssets)],
        ["Open Incidents", String(summary.openIncidents)],
        [
          "Active Inventory Campaigns",
          String(summary.activeInventoryCampaigns),
        ],
      ],
      rowsFor("Status", summary.byStatus),
      rowsFor("Condition", summary.byCondition),
      rowsFor("Branch", summary.byBranch),
      rowsFor("Category", summary.byCategory),
    ];

    const csv = blocks
      .map((block) =>
        block
          .map((row) =>
            row.map((cell) => escapeCsv(cell)).join(","),
          )
          .join("\n"),
      )
      .join("\n\n");

    const blob = new Blob([csv], {
      type: "text/csv;charset=utf-8",
    });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = `aps-aims-summary-${
      new Date().toISOString().slice(0, 10)
    }.csv`;
    link.click();

    URL.revokeObjectURL(url);
  }

  if (!summary && !error) {
    return (
      <section className="panel">
        <div className="empty-state">
          Loading reports…
        </div>
      </section>
    );
  }

  return (
    <section className="panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">Reporting</p>
          <h2>Asset summary</h2>
          <p className="muted">
            Operational asset distribution and current exception
            counts.
          </p>
        </div>

        <div className="v09-action-row">
          <button
            type="button"
            className="button secondary"
            onClick={load}
          >
            Refresh
          </button>

          <button
            type="button"
            className="button primary"
            onClick={exportCsv}
            disabled={!summary}
          >
            Export CSV
          </button>
        </div>
      </div>

      {error && <div className="alert error">{error}</div>}

      {summary && (
        <>
          <div className="v09-report-cards">
            {[
              ["Total Assets", summary.totalAssets],
              ["Active Assets", summary.activeAssets],
              ["Archived Assets", summary.archivedAssets],
              ["Open Incidents", summary.openIncidents],
              [
                "Active Inventories",
                summary.activeInventoryCampaigns,
              ],
            ].map(([label, value]) => (
              <div className="v09-report-card" key={label}>
                <span>{label}</span>
                <strong>{value}</strong>
              </div>
            ))}
          </div>

          <div className="v09-report-grid">
            {[
              ["Assets by Status", summary.byStatus],
              ["Assets by Condition", summary.byCondition],
              ["Assets by Branch", summary.byBranch],
              ["Assets by Category", summary.byCategory],
            ].map(([title, rows]) => (
              <section
                className="v09-report-table"
                key={title as string}
              >
                <h3>{title as string}</h3>

                {(rows as ReportCount[]).map((row) => (
                  <div
                    className="v09-report-row"
                    key={row.label}
                  >
                    <span>
                      {formatAssetEnum(row.label)}
                    </span>
                    <strong>{row.count}</strong>
                  </div>
                ))}
              </section>
            ))}
          </div>
        </>
      )}
    </section>
  );
}
