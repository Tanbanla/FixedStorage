using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Indexes_To_InventoryDocs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
            name: "IX_InventoryDocs_Plant",
            table: "InventoryDocs",
            column: "Plant");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_WareHouseLocation",
                table: "InventoryDocs",
                column: "WareHouseLocation");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_DepartmentName",
                table: "InventoryDocs",
                column: "DepartmentName");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_LocationName",
                table: "InventoryDocs",
                column: "LocationName");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_DocType",
                table: "InventoryDocs",
                column: "DocType");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_DocCode",
                table: "InventoryDocs",
                column: "DocCode");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_AssignedAccountId",
                table: "InventoryDocs",
                column: "AssignedAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
        name: "IX_InventoryDocs_Plant",
        table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDocs_WareHouseLocation",
                table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDocs_DepartmentName",
                table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDocs_LocationName",
                table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDocs_DocType",
                table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDocs_DocCode",
                table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDocs_AssignedAccountId",
                table: "InventoryDocs");
        }
    }
}
