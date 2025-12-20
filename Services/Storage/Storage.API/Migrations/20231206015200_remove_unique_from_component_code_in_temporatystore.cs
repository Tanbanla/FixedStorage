using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class remove_unique_from_component_code_in_temporatystore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemporaryStores_ComponentCode",
                table: "TemporaryStores");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryStores_ComponentCode",
                table: "TemporaryStores",
                column: "ComponentCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemporaryStores_ComponentCode",
                table: "TemporaryStores");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryStores_ComponentCode",
                table: "TemporaryStores",
                column: "ComponentCode",
                unique: true,
                filter: "[ComponentCode] IS NOT NULL");
        }
    }
}
