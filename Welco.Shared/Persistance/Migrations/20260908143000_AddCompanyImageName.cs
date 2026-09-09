using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyImageName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Companies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "Companies");
        }
    }
}
