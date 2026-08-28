using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APS.AIMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetCustodyWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetLocations_Branches_BranchId",
                table: "AssetLocations");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "AssetCustodyHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedFromLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnedToLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReturnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssueNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReturnNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCustodyHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetCustodyHistories_AssetLocations_IssuedFromLocationId",
                        column: x => x.IssuedFromLocationId,
                        principalTable: "AssetLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetCustodyHistories_AssetLocations_ReturnedToLocationId",
                        column: x => x.ReturnedToLocationId,
                        principalTable: "AssetLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetCustodyHistories_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetCustodyHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FromCustodianId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToCustodianId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTransactions_AssetLocations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "AssetLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransactions_AssetLocations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "AssetLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransactions_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransactions_Employees_FromCustodianId",
                        column: x => x.FromCustodianId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransactions_Employees_ToCustodianId",
                        column: x => x.ToCustodianId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetCustodyHistories_AssetId",
                table: "AssetCustodyHistories",
                column: "AssetId",
                unique: true,
                filter: "\"ReturnedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCustodyHistories_AssetId_IssuedAt",
                table: "AssetCustodyHistories",
                columns: new[] { "AssetId", "IssuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetCustodyHistories_EmployeeId",
                table: "AssetCustodyHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCustodyHistories_IssuedFromLocationId",
                table: "AssetCustodyHistories",
                column: "IssuedFromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCustodyHistories_ReturnedToLocationId",
                table: "AssetCustodyHistories",
                column: "ReturnedToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransactions_AssetId_OccurredAt",
                table: "AssetTransactions",
                columns: new[] { "AssetId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransactions_FromCustodianId",
                table: "AssetTransactions",
                column: "FromCustodianId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransactions_FromLocationId",
                table: "AssetTransactions",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransactions_ToCustodianId",
                table: "AssetTransactions",
                column: "ToCustodianId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransactions_ToLocationId",
                table: "AssetTransactions",
                column: "ToLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLocations_Branches_BranchId",
                table: "AssetLocations",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetLocations_Branches_BranchId",
                table: "AssetLocations");

            migrationBuilder.DropTable(
                name: "AssetCustodyHistories");

            migrationBuilder.DropTable(
                name: "AssetTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLocations_Branches_BranchId",
                table: "AssetLocations",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
