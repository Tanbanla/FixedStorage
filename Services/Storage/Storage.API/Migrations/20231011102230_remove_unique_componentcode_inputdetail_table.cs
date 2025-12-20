using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class remove_unique_componentcode_inputdetail_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InputDetails_ComponentCode",
                table: "InputDetails");

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_ComponentCode",
                table: "InputDetails",
                column: "ComponentCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InputDetails_ComponentCode",
                table: "InputDetails");

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_ComponentCode",
                table: "InputDetails",
                column: "ComponentCode",
                unique: true,
                filter: "[ComponentCode] IS NOT NULL");
        }
    }
}
