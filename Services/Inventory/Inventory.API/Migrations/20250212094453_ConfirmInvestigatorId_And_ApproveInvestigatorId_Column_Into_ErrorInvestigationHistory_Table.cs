using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmInvestigatorId_And_ApproveInvestigatorId_Column_Into_ErrorInvestigationHistory_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApproveInvestigatorId",
                table: "ErrorInvestigationHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmInvestigatorId",
                table: "ErrorInvestigationHistories",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproveInvestigatorId",
                table: "ErrorInvestigationHistories");

            migrationBuilder.DropColumn(
                name: "ConfirmInvestigatorId",
                table: "ErrorInvestigationHistories");
        }
    }
}
