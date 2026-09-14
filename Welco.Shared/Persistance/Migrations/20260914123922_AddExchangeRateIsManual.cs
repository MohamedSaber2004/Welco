using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRateIsManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManual",
                table: "ExchangeRates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManual",
                table: "ExchangeRates");
        }
    }
}
