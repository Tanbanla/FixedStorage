using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Plant_WarehouseLocation_DocTypeCComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isHighlight",
                table: "HistoryTypeCDetails",
                newName: "IsHighlight");

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "DocTypeCComponents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseLocation",
                table: "DocTypeCComponents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plant",
                table: "DocTypeCComponents");

            migrationBuilder.DropColumn(
                name: "WarehouseLocation",
                table: "DocTypeCComponents");

            migrationBuilder.RenameColumn(
                name: "IsHighlight",
                table: "HistoryTypeCDetails",
                newName: "isHighlight");
        }
    }
}
