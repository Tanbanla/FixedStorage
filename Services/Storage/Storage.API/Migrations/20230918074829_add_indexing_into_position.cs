using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class add_indexing_into_position : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Positions_ComponentCode",
                table: "Positions",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ComponentName",
                table: "Positions",
                column: "ComponentName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Positions_ComponentCode",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_ComponentName",
                table: "Positions");
        }
    }
}
