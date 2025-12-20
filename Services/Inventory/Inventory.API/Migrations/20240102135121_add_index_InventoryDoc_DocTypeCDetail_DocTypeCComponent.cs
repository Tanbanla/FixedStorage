using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_index_InventoryDoc_DocTypeCDetail_DocTypeCComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_Status",
                table: "InventoryDocs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCDetails_InventoryId",
                table: "DocTypeCDetails",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCDetails_ModelCode",
                table: "DocTypeCDetails",
                column: "ModelCode");

            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCComponents_InventoryId",
                table: "DocTypeCComponents",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCComponents_MainModelCode",
                table: "DocTypeCComponents",
                column: "MainModelCode");

            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCComponents_UnitModelCode",
                table: "DocTypeCComponents",
                column: "UnitModelCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryDocs_Status",
                table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "IX_DocTypeCDetails_InventoryId",
                table: "DocTypeCDetails");

            migrationBuilder.DropIndex(
                name: "IX_DocTypeCDetails_ModelCode",
                table: "DocTypeCDetails");

            migrationBuilder.DropIndex(
                name: "IX_DocTypeCComponents_InventoryId",
                table: "DocTypeCComponents");

            migrationBuilder.DropIndex(
                name: "IX_DocTypeCComponents_MainModelCode",
                table: "DocTypeCComponents");

            migrationBuilder.DropIndex(
                name: "IX_DocTypeCComponents_UnitModelCode",
                table: "DocTypeCComponents");
        }
    }
}
