using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class init_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Inventories",
                newName: "InventoryDate");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Inventories",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AuditFailPercentage",
                table: "Inventories",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "InventoryStatus",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuditTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ComponentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Factory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LocationName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Market = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Outbound = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SaleOrderNo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    QuantityOfOrder = table.Column<double>(type: "float", nullable: false),
                    QuantityOfOutbound = table.Column<double>(type: "float", nullable: false),
                    SumOfTran = table.Column<double>(type: "float", nullable: false),
                    InventoryOutput = table.Column<double>(type: "float", nullable: false),
                    AssignedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTargets_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoleType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryDocs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocType = table.Column<int>(type: "int", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AssignedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ComponentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Layout = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Plant = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    WareHouseLocation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StockType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialStock = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VendorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<double>(type: "float", nullable: false),
                    TotalQuantity = table.Column<double>(type: "float", nullable: false),
                    AccountQuantity = table.Column<double>(type: "float", nullable: false),
                    ErrorQuantity = table.Column<double>(type: "float", nullable: false),
                    UnitPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SapInventoryNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModelCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SalesOrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SaleOrderList = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductOrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InventoryBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuditBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuditAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiveBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssemblyLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    columnC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    columnN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    columnO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    columnP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    columnQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    columnR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    columnS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryDocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryDocs_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeLog = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Action = table.Column<int>(type: "int", nullable: false),
                    EvicenceImg = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InventoryDocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocHistories_InventoryDocs_InventoryDocId",
                        column: x => x.InventoryDocId,
                        principalTable: "InventoryDocs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityOfBom = table.Column<double>(type: "float", nullable: false),
                    QuantityPerBom = table.Column<double>(type: "float", nullable: false),
                    InventoryDocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocOutputs_InventoryDocs_InventoryDocId",
                        column: x => x.InventoryDocId,
                        principalTable: "InventoryDocs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocTypeCDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModelCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    QuantityOfBom = table.Column<double>(type: "float", nullable: false),
                    QuantityPerBom = table.Column<double>(type: "float", nullable: false),
                    isHighlight = table.Column<bool>(type: "bit", nullable: false),
                    InventoryDocId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocTypeCDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocTypeCDetails_InventoryDocs_InventoryDocId",
                        column: x => x.InventoryDocId,
                        principalTable: "InventoryDocs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HistoryOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityOfBom = table.Column<double>(type: "float", nullable: false),
                    QuantityPerBom = table.Column<double>(type: "float", nullable: false),
                    DocHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryOutputs_DocHistories_DocHistoryId",
                        column: x => x.DocHistoryId,
                        principalTable: "DocHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HistoryTypeCDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModelCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    QuantityOfBom = table.Column<double>(type: "float", nullable: false),
                    QuantityPerBom = table.Column<double>(type: "float", nullable: false),
                    IsHighlight = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryTypeCDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryTypeCDetails_DocHistories_DocHistoryId",
                        column: x => x.DocHistoryId,
                        principalTable: "DocHistories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTargets_ComponentCode",
                table: "AuditTargets",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTargets_InventoryId",
                table: "AuditTargets",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DocHistories_InventoryDocId",
                table: "DocHistories",
                column: "InventoryDocId");

            migrationBuilder.CreateIndex(
                name: "IX_DocOutputs_InventoryDocId",
                table: "DocOutputs",
                column: "InventoryDocId");

            migrationBuilder.CreateIndex(
                name: "IX_DocTypeCDetails_InventoryDocId",
                table: "DocTypeCDetails",
                column: "InventoryDocId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryOutputs_DocHistoryId",
                table: "HistoryOutputs",
                column: "DocHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryTypeCDetails_DocHistoryId",
                table: "HistoryTypeCDetails",
                column: "DocHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_ComponentCode",
                table: "InventoryDocs",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_InventoryId",
                table: "InventoryDocs",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocs_ModelCode",
                table: "InventoryDocs",
                column: "ModelCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTargets");

            migrationBuilder.DropTable(
                name: "DocOutputs");

            migrationBuilder.DropTable(
                name: "DocTypeCDetails");

            migrationBuilder.DropTable(
                name: "HistoryOutputs");

            migrationBuilder.DropTable(
                name: "HistoryTypeCDetails");

            migrationBuilder.DropTable(
                name: "InventoryAccounts");

            migrationBuilder.DropTable(
                name: "InventoryLocations");

            migrationBuilder.DropTable(
                name: "DocHistories");

            migrationBuilder.DropTable(
                name: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "AuditFailPercentage",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "InventoryStatus",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "InventoryDate",
                table: "Inventories",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Inventories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Inventories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Inventories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Inventories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Inventories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
