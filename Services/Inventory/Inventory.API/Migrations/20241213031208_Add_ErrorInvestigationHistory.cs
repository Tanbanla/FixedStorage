using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_ErrorInvestigationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorInvestigationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComponentName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PositionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdjustmentNo = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<double>(type: "float", nullable: false),
                    NewValue = table.Column<double>(type: "float", nullable: false),
                    ErrorCategory = table.Column<int>(type: "int", nullable: false),
                    ErrorDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Investigator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Adjuster = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmationImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorInvestigationHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_InventoryDoc_composition_error_quantity_money",
                table: "InventoryDocs",
                columns: new[] { "ErrorQuantity", "ErrorMoney" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationHistory_ComponentCode",
                table: "ErrorInvestigationHistories",
                column: "ComponentCode")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorInvestigationHistories");

            migrationBuilder.DropIndex(
                name: "IDX_InventoryDoc_composition_error_quantity_money",
                table: "InventoryDocs");
        }
    }
}
