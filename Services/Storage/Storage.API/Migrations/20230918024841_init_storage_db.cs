using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class init_storage_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PositionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartmentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FactoryName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PositionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "int", maxLength: 30, nullable: true),
                    InventoryNumber = table.Column<int>(type: "int", maxLength: 30, nullable: true),
                    PositionHistoryType = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShelfName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShelfSide = table.Column<int>(type: "int", nullable: true),
                    Cell = table.Column<int>(type: "int", nullable: true),
                    Compartment = table.Column<int>(type: "int", nullable: true),
                    Floor = table.Column<int>(type: "int", nullable: false),
                    Layout = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ComponentName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PositionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InventoryNumber = table.Column<int>(type: "int", nullable: false),
                    MinInventoryNumber = table.Column<int>(type: "int", nullable: false),
                    MaxInventoryNumber = table.Column<int>(type: "int", nullable: false),
                    ComponentInfo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Storages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Layout = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FactoryId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FactoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Storages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PositionCode",
                table: "Positions",
                column: "PositionCode",
                unique: true,
                filter: "[PositionCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Factory");

            migrationBuilder.DropTable(
                name: "PositionHistories");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Storages");
        }
    }
}
