using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Update_ReportAudit_Column_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LocationtName",
                table: "ReportingAudits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditorName",
                table: "ReportingAudits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "ReportingAudits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IDX_InventoryId_DepartmentName_LocationtName_AuditorName_COMPOSITION",
                table: "ReportingAudits",
                columns: new[] { "InventoryId", "DepartmentName", "LocationtName", "AuditorName" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ReportingAudit_InventoryId",
                table: "ReportingAudits",
                column: "InventoryId")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_InventoryId_DepartmentName_LocationtName_AuditorName_COMPOSITION",
                table: "ReportingAudits");

            migrationBuilder.DropIndex(
                name: "IDX_ReportingAudit_InventoryId",
                table: "ReportingAudits");

            migrationBuilder.DropColumn(
                name: "AuditorName",
                table: "ReportingAudits");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "ReportingAudits");

            migrationBuilder.AlterColumn<string>(
                name: "LocationtName",
                table: "ReportingAudits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
