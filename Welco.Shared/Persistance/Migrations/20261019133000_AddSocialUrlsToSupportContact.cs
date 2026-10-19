using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialUrlsToSupportContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "SupportContacts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "SupportContacts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "SupportContacts");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "SupportContacts");
        }
    }
}
