using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributorExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryInterest",
                table: "DistributorApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "DistributorApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryInterest",
                table: "DistributorApplications");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "DistributorApplications");
        }
    }
}
