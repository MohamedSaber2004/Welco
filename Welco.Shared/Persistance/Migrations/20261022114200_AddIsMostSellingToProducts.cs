using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddIsMostSellingToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS to safely add columns that may already exist from a manual SQL migration
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[Products]') 
                    AND name = 'IsMostSelling'
                )
                BEGIN
                    ALTER TABLE [Products] ADD [IsMostSelling] bit NOT NULL CONSTRAINT [DF_Products_IsMostSelling] DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[Products]') 
                    AND name = 'IsMostSelling'
                )
                BEGIN
                    DECLARE @dfName sysname;
                    SELECT @dfName = dc.name
                    FROM sys.default_constraints dc
                    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'[Products]') AND c.name = 'IsMostSelling';

                    IF @dfName IS NOT NULL
                        EXEC('ALTER TABLE [Products] DROP CONSTRAINT ' + @dfName);

                    ALTER TABLE [Products] DROP COLUMN [IsMostSelling];
                END
            ");
        }
    }
}
