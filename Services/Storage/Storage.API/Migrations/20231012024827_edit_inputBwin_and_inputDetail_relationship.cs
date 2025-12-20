using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class edit_inputBwin_and_inputDetail_relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InputDetails_InputId",
                table: "InputDetails");

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_InputId",
                table: "InputDetails",
                column: "InputId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InputDetails_InputId",
                table: "InputDetails");

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_InputId",
                table: "InputDetails",
                column: "InputId",
                unique: true,
                filter: "[InputId] IS NOT NULL");
        }
    }
}
