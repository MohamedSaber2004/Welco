using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class SyncCompanyIsProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProductInteractions_UserId_ProductId_Type",
                table: "UserProductInteractions");

            migrationBuilder.AddColumn<bool>(
                name: "IsProvider",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserProductInteractions_UserId_ProductId_Type",
                table: "UserProductInteractions",
                columns: new[] { "UserId", "ProductId", "Type" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProductInteractions_UserId_ProductId_Type",
                table: "UserProductInteractions");

            migrationBuilder.DropColumn(
                name: "IsProvider",
                table: "Companies");

            migrationBuilder.CreateIndex(
                name: "IX_UserProductInteractions_UserId_ProductId_Type",
                table: "UserProductInteractions",
                columns: new[] { "UserId", "ProductId", "Type" },
                unique: true);
        }
    }
}
