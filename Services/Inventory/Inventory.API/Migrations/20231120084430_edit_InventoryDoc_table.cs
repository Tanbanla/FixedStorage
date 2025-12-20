using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class edit_InventoryDoc_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "columnS",
                table: "InventoryDocs",
                newName: "ColumnS");

            migrationBuilder.RenameColumn(
                name: "columnR",
                table: "InventoryDocs",
                newName: "ColumnR");

            migrationBuilder.RenameColumn(
                name: "columnQ",
                table: "InventoryDocs",
                newName: "ColumnQ");

            migrationBuilder.RenameColumn(
                name: "columnP",
                table: "InventoryDocs",
                newName: "ColumnP");

            migrationBuilder.RenameColumn(
                name: "columnO",
                table: "InventoryDocs",
                newName: "ColumnO");

            migrationBuilder.RenameColumn(
                name: "columnN",
                table: "InventoryDocs",
                newName: "ColumnN");

            migrationBuilder.RenameColumn(
                name: "columnC",
                table: "InventoryDocs",
                newName: "ColumnC");

            migrationBuilder.AddColumn<double>(
                name: "BomUseQuantity",
                table: "InventoryDocs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "LineName",
                table: "InventoryDocs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineType",
                table: "InventoryDocs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineModel",
                table: "InventoryDocs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineType",
                table: "InventoryDocs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StageName",
                table: "InventoryDocs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StageNumber",
                table: "InventoryDocs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BomUseQuantity",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "LineName",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "LineType",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "MachineModel",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "MachineType",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "StageName",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "StageNumber",
                table: "InventoryDocs");

            migrationBuilder.RenameColumn(
                name: "ColumnS",
                table: "InventoryDocs",
                newName: "columnS");

            migrationBuilder.RenameColumn(
                name: "ColumnR",
                table: "InventoryDocs",
                newName: "columnR");

            migrationBuilder.RenameColumn(
                name: "ColumnQ",
                table: "InventoryDocs",
                newName: "columnQ");

            migrationBuilder.RenameColumn(
                name: "ColumnP",
                table: "InventoryDocs",
                newName: "columnP");

            migrationBuilder.RenameColumn(
                name: "ColumnO",
                table: "InventoryDocs",
                newName: "columnO");

            migrationBuilder.RenameColumn(
                name: "ColumnN",
                table: "InventoryDocs",
                newName: "columnN");

            migrationBuilder.RenameColumn(
                name: "ColumnC",
                table: "InventoryDocs",
                newName: "columnC");
        }
    }
}
