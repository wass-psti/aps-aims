using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APS.AIMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceAndCalibration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetCalibrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceProvider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NextCalibrationDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCalibrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetCalibrations_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetMaintenances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ServiceProvider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    NextMaintenanceDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMaintenances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetMaintenances_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetCalibrations_AssetId",
                table: "AssetCalibrations",
                column: "AssetId",
                unique: true,
                filter: "\"CompletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCalibrations_AssetId_StartedAt",
                table: "AssetCalibrations",
                columns: new[] { "AssetId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenances_AssetId",
                table: "AssetMaintenances",
                column: "AssetId",
                unique: true,
                filter: "\"CompletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaintenances_AssetId_StartedAt",
                table: "AssetMaintenances",
                columns: new[] { "AssetId", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetCalibrations");

            migrationBuilder.DropTable(
                name: "AssetMaintenances");
        }
    }
}
