using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_No_column_DocTypeCDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "No",
                table: "DocTypeCDetails",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "No",
                table: "DocTypeCDetails");
        }
    }
}
