using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Unique_PositionCode_Into_Table_Positions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Positions_PositionCode",
                table: "Positions");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PositionCode",
                table: "Positions",
                column: "PositionCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Positions_PositionCode",
                table: "Positions");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PositionCode",
                table: "Positions",
                column: "PositionCode",
                unique: true,
                filter: "[PositionCode] IS NOT NULL");
        }
    }
}
