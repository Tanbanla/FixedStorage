using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_Reporting_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportingAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    LocationtName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalDoc = table.Column<int>(type: "int", nullable: false),
                    TotalTodo = table.Column<int>(type: "int", nullable: false),
                    TotalPass = table.Column<int>(type: "int", nullable: false),
                    TotalFail = table.Column<int>(type: "int", nullable: false),
                    CaptureTimeType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalDoc = table.Column<int>(type: "int", nullable: false),
                    TotalTodo = table.Column<int>(type: "int", nullable: false),
                    TotalInventory = table.Column<int>(type: "int", nullable: false),
                    TotalConfirm = table.Column<int>(type: "int", nullable: false),
                    CaptureTimeType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingDepartments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingDocTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    TotalDoc = table.Column<int>(type: "int", nullable: false),
                    TotalTodo = table.Column<int>(type: "int", nullable: false),
                    TotalInventory = table.Column<int>(type: "int", nullable: false),
                    TotalConfirm = table.Column<int>(type: "int", nullable: false),
                    CaptureTimeType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingDocTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalDoc = table.Column<int>(type: "int", nullable: false),
                    TotalTodo = table.Column<int>(type: "int", nullable: false),
                    TotalInventory = table.Column<int>(type: "int", nullable: false),
                    TotalConfirm = table.Column<int>(type: "int", nullable: false),
                    CaptureTimeType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingLocations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportingAudits");

            migrationBuilder.DropTable(
                name: "ReportingDepartments");

            migrationBuilder.DropTable(
                name: "ReportingDocTypes");

            migrationBuilder.DropTable(
                name: "ReportingLocations");
        }
    }
}
