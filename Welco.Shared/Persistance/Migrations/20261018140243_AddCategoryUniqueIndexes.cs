using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Currencies",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId_NameAr",
                table: "Categories",
                columns: new[] { "ParentCategoryId", "NameAr" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId_NameEn",
                table: "Categories",
                columns: new[] { "ParentCategoryId", "NameEn" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId_NameAr",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId_NameEn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Currencies");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");
        }
    }
}
