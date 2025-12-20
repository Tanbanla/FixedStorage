using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class Update_foreignkey_BwinHistory_And_PostionHistory_And_Position : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "BwinHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PositionHistories_PositionId",
                table: "PositionHistories",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_BwinHistories_PositionId",
                table: "BwinHistories",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_BwinHistories_Positions_PositionId",
                table: "BwinHistories",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionHistories_Positions_PositionId",
                table: "PositionHistories",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BwinHistories_Positions_PositionId",
                table: "BwinHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionHistories_Positions_PositionId",
                table: "PositionHistories");

            migrationBuilder.DropIndex(
                name: "IX_PositionHistories_PositionId",
                table: "PositionHistories");

            migrationBuilder.DropIndex(
                name: "IX_BwinHistories_PositionId",
                table: "BwinHistories");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "BwinHistories");
        }
    }
}
