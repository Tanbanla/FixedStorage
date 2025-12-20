using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class add_bwin_table_and_input_detail_and_temporary_store : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BwinHistories");

            migrationBuilder.CreateTable(
                name: "InputDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InputId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    BwinOutputCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ComponentCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SuplierCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PositionCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RemainingHandle = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_InputDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InputFromBwins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputFromBwins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BwinOutputCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ComponentCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryStores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_BwinOutputCode",
                table: "InputDetails",
                column: "BwinOutputCode");

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_ComponentCode",
                table: "InputDetails",
                column: "ComponentCode",
                unique: true,
                filter: "[ComponentCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_PositionCode",
                table: "InputDetails",
                column: "PositionCode");

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_SuplierCode",
                table: "InputDetails",
                column: "SuplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryStores_BwinOutputCode",
                table: "TemporaryStores",
                column: "BwinOutputCode");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryStores_ComponentCode",
                table: "TemporaryStores",
                column: "ComponentCode",
                unique: true,
                filter: "[ComponentCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryStores_SupplierCode",
                table: "TemporaryStores",
                column: "SupplierCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InputDetails");

            migrationBuilder.DropTable(
                name: "InputFromBwins");

            migrationBuilder.DropTable(
                name: "TemporaryStores");

            migrationBuilder.CreateTable(
                name: "BwinHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DispatchOrderCodeQty = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalQtyInventoryComponent = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BwinHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BwinHistories_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BwinHistories_PositionId",
                table: "BwinHistories",
                column: "PositionId");
        }
    }
}
