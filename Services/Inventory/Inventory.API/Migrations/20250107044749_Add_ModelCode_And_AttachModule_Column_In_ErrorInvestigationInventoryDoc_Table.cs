using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_ModelCode_And_AttachModule_Column_In_ErrorInvestigationInventoryDoc_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachModule",
                table: "ErrorInvestigationInventoryDocs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModelCode",
                table: "ErrorInvestigationInventoryDocs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachModule",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "ModelCode",
                table: "ErrorInvestigationInventoryDocs");
        }
    }
}
