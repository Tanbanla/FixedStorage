using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Investigator_And_ReasonInvestigator_And_InvestigateTime_Column_Into_InventoryDocs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InvestigateTime",
                table: "InventoryDocs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Investigator",
                table: "InventoryDocs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonInvestigator",
                table: "InventoryDocs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvestigateTime",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "Investigator",
                table: "InventoryDocs");

            migrationBuilder.DropColumn(
                name: "ReasonInvestigator",
                table: "InventoryDocs");
        }
    }
}
