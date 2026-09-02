using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APS.AIMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryIncidentsAndReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolutionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetIncidents_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryCampaigns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservedLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemCondition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ObservedCondition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Result = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CountedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_AssetLocations_ObservedLocationId",
                        column: x => x.ObservedLocationId,
                        principalTable: "AssetLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_AssetLocations_SystemLocationId",
                        column: x => x.SystemLocationId,
                        principalTable: "AssetLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_InventoryCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "InventoryCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetIncidents_AssetId_ReportedAt",
                table: "AssetIncidents",
                columns: new[] { "AssetId", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetIncidents_Status",
                table: "AssetIncidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCampaigns_BranchId_Status",
                table: "InventoryCampaigns",
                columns: new[] { "BranchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_AssetId",
                table: "InventoryCounts",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_CampaignId_AssetId",
                table: "InventoryCounts",
                columns: new[] { "CampaignId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_CampaignId_CountedAt",
                table: "InventoryCounts",
                columns: new[] { "CampaignId", "CountedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_ObservedLocationId",
                table: "InventoryCounts",
                column: "ObservedLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_SystemLocationId",
                table: "InventoryCounts",
                column: "SystemLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetIncidents");

            migrationBuilder.DropTable(
                name: "InventoryCounts");

            migrationBuilder.DropTable(
                name: "InventoryCampaigns");
        }
    }
}
