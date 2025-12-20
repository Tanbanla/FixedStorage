using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_index_DocTypeCComponent_InventoryDocId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCComponents_InventoryDocId",
                table: "DocTypeCComponents",
                column: "InventoryDocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocTypeCComponents_InventoryDocId",
                table: "DocTypeCComponents");
        }
    }
}
