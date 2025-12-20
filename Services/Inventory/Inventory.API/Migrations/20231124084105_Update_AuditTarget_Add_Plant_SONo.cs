using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Update_AuditTarget_Add_Plant_SONo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocHistories_InventoryDocs_InventoryDocId",
                table: "DocHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryDocs_Inventories_InventoryId",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "ErrorMoney",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "ErrorQuantity",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "AuditTargets");

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "AuditTargets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaleOrderNo",
                table: "AuditTargets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocHistories_InventoryDocs_InventoryDocId",
                table: "DocHistories",
                column: "InventoryDocId",
                principalTable: "InventoryDocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryDocs_Inventories_InventoryId",
                table: "InventoryDocs",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocHistories_InventoryDocs_InventoryDocId",
                table: "DocHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryDocs_Inventories_InventoryId",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "Plant",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "SaleOrderNo",
                table: "AuditTargets");

            migrationBuilder.AddColumn<double>(
                name: "ErrorMoney",
                table: "AuditTargets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ErrorQuantity",
                table: "AuditTargets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UnitPrice",
                table: "AuditTargets",
                type: "float",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocHistories_InventoryDocs_InventoryDocId",
                table: "DocHistories",
                column: "InventoryDocId",
                principalTable: "InventoryDocs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryDocs_Inventories_InventoryId",
                table: "InventoryDocs",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id");
        }
    }
}
