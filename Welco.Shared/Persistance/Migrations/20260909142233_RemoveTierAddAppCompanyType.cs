using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
        public partial class RemoveTierAddAppCompanyType : Migration
    {
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

migrationBuilder.Sql("UPDATE [Companies] SET [Type] = 'Distributor' WHERE [Type] = 'Importer'");
        }

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
