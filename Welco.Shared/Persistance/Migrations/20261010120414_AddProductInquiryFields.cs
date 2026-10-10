using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Welco.Shared.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInquiryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS to safely add columns that may already exist from a manual SQL migration
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RFQs') AND name = 'ResponseNote')
                    ALTER TABLE [RFQs] ADD [ResponseNote] nvarchar(max) NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'Note')
                    ALTER TABLE [Quotes] ADD [Note] nvarchar(max) NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'RespondedAt')
                    ALTER TABLE [ProductInquiries] ADD [RespondedAt] datetime2 NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'RespondedById')
                    ALTER TABLE [ProductInquiries] ADD [RespondedById] uniqueidentifier NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'Response')
                    ALTER TABLE [ProductInquiries] ADD [Response] nvarchar(max) NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'Status')
                    ALTER TABLE [ProductInquiries] ADD [Status] int NOT NULL DEFAULT 1;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'UserId')
                    ALTER TABLE [ProductInquiries] ADD [UserId] uniqueidentifier NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'IX_ProductInquiries_RespondedById')
                    CREATE INDEX [IX_ProductInquiries_RespondedById] ON [ProductInquiries] ([RespondedById]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'IX_ProductInquiries_UserId')
                    CREATE INDEX [IX_ProductInquiries_UserId] ON [ProductInquiries] ([UserId]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProductInquiries_Users_RespondedById') ALTER TABLE [ProductInquiries] DROP CONSTRAINT [FK_ProductInquiries_Users_RespondedById];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProductInquiries_Users_UserId') ALTER TABLE [ProductInquiries] DROP CONSTRAINT [FK_ProductInquiries_Users_UserId];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'IX_ProductInquiries_RespondedById') DROP INDEX [IX_ProductInquiries_RespondedById] ON [ProductInquiries];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'IX_ProductInquiries_UserId') DROP INDEX [IX_ProductInquiries_UserId] ON [ProductInquiries];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RFQs') AND name = 'ResponseNote') ALTER TABLE [RFQs] DROP COLUMN [ResponseNote];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotes') AND name = 'Note') ALTER TABLE [Quotes] DROP COLUMN [Note];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'RespondedAt') ALTER TABLE [ProductInquiries] DROP COLUMN [RespondedAt];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'RespondedById') ALTER TABLE [ProductInquiries] DROP COLUMN [RespondedById];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'Response') ALTER TABLE [ProductInquiries] DROP COLUMN [Response];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'Status') ALTER TABLE [ProductInquiries] DROP COLUMN [Status];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductInquiries') AND name = 'UserId') ALTER TABLE [ProductInquiries] DROP COLUMN [UserId];");
        }
    }
}
