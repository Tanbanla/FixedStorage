using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_composition_index_inventorydocs_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "InventoryDoc_composition_doctype_status",
                table: "InventoryDocs",
                columns: new[] { "DocType", "Status" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "DepartmentName", "LocationName", "InventoryId" });

            migrationBuilder.CreateIndex(
                name: "InventoryDoc_CreatedAt",
                table: "InventoryDocs",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "InventoryDoc_composition_doctype_status",
                table: "InventoryDocs");

            migrationBuilder.DropIndex(
                name: "InventoryDoc_CreatedAt",
                table: "InventoryDocs");
        }
    }
}
