using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Update_InventoryDoc_InventoryLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "AuditTargets");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "InventoryDocs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "InventoryAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccounts_LocationId",
                table: "InventoryAccounts",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAccounts_InventoryLocations_LocationId",
                table: "InventoryAccounts",
                column: "LocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAccounts_InventoryLocations_LocationId",
                table: "InventoryAccounts");

            migrationBuilder.DropIndex(
                name: "IX_InventoryAccounts_LocationId",
                table: "InventoryAccounts");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "InventoryAccounts");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "AuditTargets",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
