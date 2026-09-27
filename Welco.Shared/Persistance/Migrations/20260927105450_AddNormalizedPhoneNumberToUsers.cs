using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedPhoneNumberToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedPhoneNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Backfill the canonical key for existing accounts so the new
            // uniqueness rule also covers rows created before this change.
            // Mirrors PhoneNumberNormalizer: drop separators, keep a leading
            // "+", and rewrite a leading "00" international prefix to "+".
            migrationBuilder.Sql(@"
UPDATE [Users]
SET [NormalizedPhoneNumber] =
    CASE
        WHEN LEFT(LTRIM(RTRIM([PhoneNumber])), 2) = '00'
            THEN '+' + SUBSTRING(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM([PhoneNumber])), '+', ''), ' ', ''), '-', ''), 3, 20)
        WHEN LEFT(LTRIM(RTRIM([PhoneNumber])), 1) = '+'
            THEN '+' + REPLACE(REPLACE(SUBSTRING(LTRIM(RTRIM([PhoneNumber])), 2, 20), '+', ''), ' ', '')
        ELSE REPLACE(REPLACE(REPLACE(LTRIM(RTRIM([PhoneNumber])), '+', ''), ' ', ''), '-', '')
    END
WHERE [PhoneNumber] IS NOT NULL
  AND LTRIM(RTRIM([PhoneNumber])) <> '';");

            // If legacy data already contains the same number more than once,
            // keep the earliest account's key and clear the rest. Nothing is
            // deleted — it only prevents the unique index from failing, and
            // those rows can be reconciled manually.
            migrationBuilder.Sql(@"
WITH dups AS (
    SELECT [NormalizedPhoneNumber], MIN([CreatedAt]) AS KeepAt
    FROM [Users]
    WHERE [NormalizedPhoneNumber] IS NOT NULL
    GROUP BY [NormalizedPhoneNumber]
    HAVING COUNT(*) > 1
)
UPDATE u
SET u.[NormalizedPhoneNumber] = NULL
FROM [Users] u
INNER JOIN dups d ON u.[NormalizedPhoneNumber] = d.[NormalizedPhoneNumber]
WHERE u.[CreatedAt] > d.KeepAt OR u.[CreatedAt] IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedPhoneNumber",
                table: "Users",
                column: "NormalizedPhoneNumber",
                unique: true,
                filter: "[NormalizedPhoneNumber] IS NOT NULL AND [NormalizedPhoneNumber] <> '' AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedPhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedPhoneNumber",
                table: "Users");
        }
    }
}
