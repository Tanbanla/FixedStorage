using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class update_inventoryaccount_set_nullable_role_and_locationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAccounts_InventoryLocations_LocationId",
                table: "InventoryAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "RoleType",
                table: "InventoryAccounts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "InventoryAccounts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAccounts_InventoryLocations_LocationId",
                table: "InventoryAccounts",
                column: "LocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAccounts_InventoryLocations_LocationId",
                table: "InventoryAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "RoleType",
                table: "InventoryAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "InventoryAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAccounts_InventoryLocations_LocationId",
                table: "InventoryAccounts",
                column: "LocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
