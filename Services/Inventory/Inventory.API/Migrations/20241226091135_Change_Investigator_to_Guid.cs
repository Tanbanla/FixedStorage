using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Change_Investigator_to_Guid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Investigator",
                table: "ErrorInvestigations");

            migrationBuilder.AddColumn<Guid>(
                name: "InvestigatorId",
                table: "ErrorInvestigations",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvestigatorId",
                table: "ErrorInvestigations");

            migrationBuilder.AddColumn<string>(
                name: "Investigator",
                table: "ErrorInvestigations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
