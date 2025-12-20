using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_ErrorInvestigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConfirmationImage",
                table: "ErrorInvestigationHistories",
                newName: "ConfirmationImage1");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationImage2",
                table: "ErrorInvestigationHistories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ErrorInvestigationId",
                table: "ErrorInvestigationHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ErrorInvestigations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    Plant = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WareHouseLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComponentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PositionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalQuantity = table.Column<double>(type: "float", nullable: false),
                    AccountQuantity = table.Column<double>(type: "float", nullable: false),
                    ErrorQuantity = table.Column<double>(type: "float", nullable: false),
                    ErrorMoney = table.Column<double>(type: "float", nullable: false),
                    UnitPrice = table.Column<double>(type: "float", nullable: false),
                    AssignedAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InventoryBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdjustedQuantity = table.Column<double>(type: "float", nullable: true),
                    Investigator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjustmentNo = table.Column<int>(type: "int", nullable: false),
                    QuantityDifference = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorInvestigations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorInvestigations_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ErrorInvestigationInventoryDocs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErrorInvestigationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryDocId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorInvestigationInventoryDocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorInvestigationInventoryDocs_ErrorInvestigations_ErrorInvestigationId",
                        column: x => x.ErrorInvestigationId,
                        principalTable: "ErrorInvestigations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErrorInvestigationInventoryDocs_InventoryDocs_InventoryDocId",
                        column: x => x.InventoryDocId,
                        principalTable: "InventoryDocs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorInvestigationHistories_ErrorInvestigationId",
                table: "ErrorInvestigationHistories",
                column: "ErrorInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorInvestigationInventoryDocs_ErrorInvestigationId",
                table: "ErrorInvestigationInventoryDocs",
                column: "ErrorInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorInvestigationInventoryDocs_InventoryDocId",
                table: "ErrorInvestigationInventoryDocs",
                column: "InventoryDocId");

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigation_ComponentCode",
                table: "ErrorInvestigations",
                column: "ComponentCode")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ErrorInvestigations_InventoryId",
                table: "ErrorInvestigations",
                column: "InventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorInvestigationHistories_ErrorInvestigations_ErrorInvestigationId",
                table: "ErrorInvestigationHistories",
                column: "ErrorInvestigationId",
                principalTable: "ErrorInvestigations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ErrorInvestigationHistories_ErrorInvestigations_ErrorInvestigationId",
                table: "ErrorInvestigationHistories");

            migrationBuilder.DropTable(
                name: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropTable(
                name: "ErrorInvestigations");

            migrationBuilder.DropIndex(
                name: "IX_ErrorInvestigationHistories_ErrorInvestigationId",
                table: "ErrorInvestigationHistories");

            migrationBuilder.DropColumn(
                name: "ConfirmationImage2",
                table: "ErrorInvestigationHistories");

            migrationBuilder.DropColumn(
                name: "ErrorInvestigationId",
                table: "ErrorInvestigationHistories");

            migrationBuilder.RenameColumn(
                name: "ConfirmationImage1",
                table: "ErrorInvestigationHistories",
                newName: "ConfirmationImage");
        }
    }
}
