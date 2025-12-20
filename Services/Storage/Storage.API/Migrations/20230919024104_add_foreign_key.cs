using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class add_foreign_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Storages_FactoryId",
                table: "Storages",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_StorageId",
                table: "Positions",
                column: "StorageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Storages_StorageId",
                table: "Positions",
                column: "StorageId",
                principalTable: "Storages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Storages_Factory_FactoryId",
                table: "Storages",
                column: "FactoryId",
                principalTable: "Factory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Storages_StorageId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Storages_Factory_FactoryId",
                table: "Storages");

            migrationBuilder.DropIndex(
                name: "IX_Storages_FactoryId",
                table: "Storages");

            migrationBuilder.DropIndex(
                name: "IX_Positions_StorageId",
                table: "Positions");
        }
    }
}
