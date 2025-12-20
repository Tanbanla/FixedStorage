using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_column_ErrorQtyAbs_and_ErrorMoneyAbs_into_InventoryDocs_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ErrorMoneyAbs",
                table: "InventoryDocs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ErrorQtyAbs",
                table: "InventoryDocs",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMoneyAbs",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "ErrorQtyAbs",
                table: "InventoryDocs");
        }
    }
}
