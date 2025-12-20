using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class add_indexes_for_errorinvestigation_and_errorinvestioninventorydocs_and_errorinvestigationhistories_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigation_Status",
                table: "ErrorInvestigations",
                column: "Status")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_AccountQuantity",
                table: "ErrorInvestigationInventoryDocs",
                column: "AccountQuantity")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_AssignedAccount",
                table: "ErrorInvestigationInventoryDocs",
                column: "AssignedAccount")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_ErrorQuantity",
                table: "ErrorInvestigationInventoryDocs",
                column: "ErrorQuantity")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_Plant",
                table: "ErrorInvestigationInventoryDocs",
                column: "Plant")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_PositionCode",
                table: "ErrorInvestigationInventoryDocs",
                column: "PositionCode")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_WareHouseLocation",
                table: "ErrorInvestigationInventoryDocs",
                column: "WareHouseLocation")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IDX_ErrorInvestigationHistory_ErrorCategory",
                table: "ErrorInvestigationHistories",
                column: "ErrorCategory")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigation_Status",
                table: "ErrorInvestigations");

            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_AccountQuantity",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_AssignedAccount",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_ErrorQuantity",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_Plant",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_PositionCode",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigationInventoryDoc_WareHouseLocation",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropIndex(
                name: "IDX_ErrorInvestigationHistory_ErrorCategory",
                table: "ErrorInvestigationHistories");
        }
    }
}
