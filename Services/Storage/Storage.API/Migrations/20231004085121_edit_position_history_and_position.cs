using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class edit_position_history_and_position : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionHistories_Positions_PositionId",
                table: "PositionHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "FactoryId",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PositionId",
                table: "PositionHistories",
                type: "uniqueidentifier",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "AppUserId",
                table: "PositionHistories",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ComponentCode",
                table: "PositionHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComponentName",
                table: "PositionHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FactoryId",
                table: "PositionHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "PositionHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "PositionHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeOfBusiness",
                table: "PositionHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionHistories_Positions_PositionId",
                table: "PositionHistories",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionHistories_Positions_PositionId",
                table: "PositionHistories");

            migrationBuilder.DropColumn(
                name: "FactoryId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ComponentCode",
                table: "PositionHistories");

            migrationBuilder.DropColumn(
                name: "ComponentName",
                table: "PositionHistories");

            migrationBuilder.DropColumn(
                name: "FactoryId",
                table: "PositionHistories");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "PositionHistories");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "PositionHistories");

            migrationBuilder.DropColumn(
                name: "TypeOfBusiness",
                table: "PositionHistories");

            migrationBuilder.AlterColumn<Guid>(
                name: "PositionId",
                table: "PositionHistories",
                type: "uniqueidentifier",
                maxLength: 50,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AppUserId",
                table: "PositionHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionHistories_Positions_PositionId",
                table: "PositionHistories",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
