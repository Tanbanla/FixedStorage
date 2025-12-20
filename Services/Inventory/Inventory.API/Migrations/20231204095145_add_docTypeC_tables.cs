using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_docTypeC_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocTypeCComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryDocId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    MainModelCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UnitModelCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QuantityOfBOM = table.Column<int>(type: "int", nullable: false),
                    QuantityPerBOM = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocTypeCComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocTypeCUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Plant = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    WarehouseLocation = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    MachineModel = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    MachineType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    LineName = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    lineType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    StageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StageNumeber = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocTypeCUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocTypeCUnitDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocTypeCUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    ModelCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QuantityOfBOM = table.Column<int>(type: "int", nullable: false),
                    QuantityPerBOM = table.Column<int>(type: "int", nullable: false),
                    IsHighLight = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocTypeCUnitDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocTypeCUnitDetails_DocTypeCUnits_DocTypeCUnitId",
                        column: x => x.DocTypeCUnitId,
                        principalTable: "DocTypeCUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCUnitDetails_DocTypeCUnitId",
                table: "DocTypeCUnitDetails",
                column: "DocTypeCUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocTypeCComponents");

            migrationBuilder.DropTable(
                name: "DocTypeCUnitDetails");

            migrationBuilder.DropTable(
                name: "DocTypeCUnits");
        }
    }
}
