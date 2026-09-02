using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APS.AIMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImmutableAuditLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    UserDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Resource = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAt",
                table: "AuditLogs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Resource_OccurredAt",
                table: "AuditLogs",
                columns: new[] { "Resource", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
