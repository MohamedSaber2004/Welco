using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTierAddAppCompanyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TierLevel",
                table: "Companies");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "DistributorApplications",
                type: "int",
                nullable: false,
                defaultValue: 2);

            // CompanyType.Importer was removed from the enum (Company.Type is
            // string-converted) — fold legacy rows into Distributor.
            migrationBuilder.Sql("UPDATE [Companies] SET [Type] = 'Distributor' WHERE [Type] = 'Importer'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "DistributorApplications");

            migrationBuilder.AddColumn<int>(
                name: "TierLevel",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
