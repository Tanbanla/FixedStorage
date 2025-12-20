using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class add_position_code_in_PostionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PositionCode",
                table: "PositionHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionHistories_PositionCode",
                table: "PositionHistories",
                column: "PositionCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PositionHistories_PositionCode",
                table: "PositionHistories");

            migrationBuilder.DropColumn(
                name: "PositionCode",
                table: "PositionHistories");
        }
    }
}
