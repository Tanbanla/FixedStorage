using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Update_AuditTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditTargets_Inventories_InventoryId",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "Factory",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "InventoryOutput",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "Market",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "Outbound",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "QuantityOfOrder",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "QuantityOfOutbound",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "SumOfTran",
                table: "AuditTargets");

            migrationBuilder.RenameColumn(
                name: "Layout",
                table: "InventoryDocs",
                newName: "PositionCode");

            migrationBuilder.RenameColumn(
                name: "SaleOrderNo",
                table: "AuditTargets",
                newName: "PositionCode");

            migrationBuilder.AlterColumn<Guid>(
                name: "InventoryId",
                table: "AuditTargets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedAccountId",
                table: "AuditTargets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ErrorMoney",
                table: "AuditTargets",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ErrorQuantity",
                table: "AuditTargets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "AuditTargets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UnitPrice",
                table: "AuditTargets",
                type: "float",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditTargets_Inventories_InventoryId",
                table: "AuditTargets",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditTargets_Inventories_InventoryId",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "ErrorMoney",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "ErrorQuantity",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "AuditTargets");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "AuditTargets");

            migrationBuilder.RenameColumn(
                name: "PositionCode",
                table: "InventoryDocs",
                newName: "Layout");

            migrationBuilder.RenameColumn(
                name: "PositionCode",
                table: "AuditTargets",
                newName: "SaleOrderNo");

            migrationBuilder.AlterColumn<Guid>(
                name: "InventoryId",
                table: "AuditTargets",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedAccountId",
                table: "AuditTargets",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Factory",
                table: "AuditTargets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "InventoryOutput",
                table: "AuditTargets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Market",
                table: "AuditTargets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outbound",
                table: "AuditTargets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "AuditTargets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityOfOrder",
                table: "AuditTargets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "QuantityOfOutbound",
                table: "AuditTargets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SumOfTran",
                table: "AuditTargets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditTargets_Inventories_InventoryId",
                table: "AuditTargets",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id");
        }
    }
}
