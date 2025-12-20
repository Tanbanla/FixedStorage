using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnIntoDocHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeLog",
                table: "DocHistories");

            migrationBuilder.AddColumn<bool>(
                name: "IsChangeCDetail",
                table: "DocHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "NewQuantity",
                table: "DocHistories",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "NewStatus",
                table: "DocHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "OldQuantity",
                table: "DocHistories",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "OldStatus",
                table: "DocHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DocHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChangeCDetail",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "NewQuantity",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "NewStatus",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "OldQuantity",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "OldStatus",
                table: "DocHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DocHistories");

            migrationBuilder.AddColumn<string>(
                name: "ChangeLog",
                table: "DocHistories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
