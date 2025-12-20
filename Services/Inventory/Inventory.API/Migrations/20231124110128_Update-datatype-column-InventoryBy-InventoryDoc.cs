using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedatatypecolumnInventoryByInventoryDoc : Migration
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

            migrationBuilder.AlterColumn<string>(
                name: "InventoryBy",
                table: "InventoryDocs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<Guid>(
                name: "InventoryBy",
                table: "InventoryDocs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
