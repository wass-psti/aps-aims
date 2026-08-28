using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APS.AIMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "AssetIdSequence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "AssetIdSequence");
        }
    }
}
