using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_audit_entity_all_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InventoryLocations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InventoryLocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "InventoryLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "InventoryLocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InventoryLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "InventoryLocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InventoryDocs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InventoryDocs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "InventoryDocs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "InventoryDocs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InventoryDocs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "InventoryDocs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InventoryAccounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InventoryAccounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "InventoryAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "InventoryAccounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InventoryAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "InventoryAccounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Inventories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Inventories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Inventories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Inventories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Inventories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Inventories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HistoryTypeCDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "HistoryTypeCDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HistoryTypeCDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "HistoryTypeCDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "HistoryTypeCDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "HistoryTypeCDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HistoryOutputs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "HistoryOutputs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "HistoryOutputs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "HistoryOutputs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "HistoryOutputs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "HistoryOutputs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DocTypeCDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DocTypeCDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DocTypeCDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DocTypeCDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DocTypeCDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "DocTypeCDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DocOutputs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DocOutputs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DocOutputs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DocOutputs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DocOutputs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "DocOutputs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DocHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DocHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DocHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DocHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DocHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "DocHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AuditTargets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AuditTargets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AuditTargets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "AuditTargets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AuditTargets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AuditTargets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InventoryLocations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InventoryLocations");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "InventoryLocations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "InventoryLocations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InventoryLocations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InventoryLocations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InventoryAccounts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InventoryAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "InventoryAccounts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "InventoryAccounts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InventoryAccounts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InventoryAccounts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HistoryTypeCDetails");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HistoryTypeCDetails");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HistoryTypeCDetails");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HistoryTypeCDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "HistoryTypeCDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "HistoryTypeCDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HistoryOutputs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HistoryOutputs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "HistoryOutputs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "HistoryOutputs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "HistoryOutputs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "HistoryOutputs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DocTypeCDetails");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DocTypeCDetails");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DocTypeCDetails");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DocTypeCDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DocTypeCDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "DocTypeCDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DocOutputs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DocOutputs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DocOutputs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DocOutputs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DocOutputs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "DocOutputs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AuditTargets");
        }
    }
}
