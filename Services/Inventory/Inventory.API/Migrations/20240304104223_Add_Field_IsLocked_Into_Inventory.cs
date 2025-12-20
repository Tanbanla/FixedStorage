using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_IsLocked_Into_Inventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Inventories",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Inventories");
        }
    }
}
