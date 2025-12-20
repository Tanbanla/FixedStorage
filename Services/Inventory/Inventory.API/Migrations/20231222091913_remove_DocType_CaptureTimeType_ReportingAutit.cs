using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class remove_DocType_CaptureTimeType_ReportingAutit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaptureTimeType",
                table: "ReportingAudits");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "ReportingAudits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaptureTimeType",
                table: "ReportingAudits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocType",
                table: "ReportingAudits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
