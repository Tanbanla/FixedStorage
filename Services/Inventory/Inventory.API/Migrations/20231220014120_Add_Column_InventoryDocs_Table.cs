using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Column_InventoryDocs_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CSAP",
                table: "InventoryDocs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ErrorMoney",
                table: "InventoryDocs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KSAP",
                table: "InventoryDocs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MSAP",
                table: "InventoryDocs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OSAP",
                table: "InventoryDocs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CSAP",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "ErrorMoney",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "KSAP",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "MSAP",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "OSAP",
                table: "InventoryDocs");
        }
    }
}
