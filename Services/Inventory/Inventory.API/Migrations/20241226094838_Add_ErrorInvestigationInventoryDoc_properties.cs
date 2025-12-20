using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_ErrorInvestigationInventoryDoc_properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocCode",
                table: "ErrorInvestigations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BOM",
                table: "ErrorInvestigationInventoryDocs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocCode",
                table: "ErrorInvestigationInventoryDocs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DocType",
                table: "ErrorInvestigationInventoryDocs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocCode",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "BOM",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "DocCode",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "ErrorInvestigationInventoryDocs");
        }
    }
}
