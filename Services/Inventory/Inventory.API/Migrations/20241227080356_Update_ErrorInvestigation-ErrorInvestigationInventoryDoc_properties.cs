using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.API.Migrations
{
    /// <inheritdoc />
    public partial class Update_ErrorInvestigationErrorInvestigationInventoryDoc_properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountQuantity",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "AdjustedQuantity",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "AssignedAccount",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "DocCode",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "DocType",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "ErrorMoney",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "ErrorQuantity",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "InventoryBy",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "Plant",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "PositionCode",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "QuantityDifference",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ErrorInvestigations");

            migrationBuilder.DropColumn(
                name: "WareHouseLocation",
                table: "ErrorInvestigations");

            migrationBuilder.AddColumn<double>(
                name: "AccountQuantity",
                table: "ErrorInvestigationInventoryDocs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AdjustedQuantity",
                table: "ErrorInvestigationInventoryDocs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedAccount",
                table: "ErrorInvestigationInventoryDocs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ErrorMoney",
                table: "ErrorInvestigationInventoryDocs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ErrorQuantity",
                table: "ErrorInvestigationInventoryDocs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "InventoryBy",
                table: "ErrorInvestigationInventoryDocs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "ErrorInvestigationInventoryDocs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PositionCode",
                table: "ErrorInvestigationInventoryDocs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityDifference",
                table: "ErrorInvestigationInventoryDocs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalQuantity",
                table: "ErrorInvestigationInventoryDocs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UnitPrice",
                table: "ErrorInvestigationInventoryDocs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "WareHouseLocation",
                table: "ErrorInvestigationInventoryDocs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountQuantity",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "AdjustedQuantity",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "AssignedAccount",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "ErrorMoney",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "ErrorQuantity",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "InventoryBy",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "Plant",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "PositionCode",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "QuantityDifference",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.DropColumn(
                name: "WareHouseLocation",
                table: "ErrorInvestigationInventoryDocs");

            migrationBuilder.AddColumn<double>(
                name: "AccountQuantity",
                table: "ErrorInvestigations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AdjustedQuantity",
                table: "ErrorInvestigations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedAccount",
                table: "ErrorInvestigations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocCode",
                table: "ErrorInvestigations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DocType",
                table: "ErrorInvestigations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ErrorMoney",
                table: "ErrorInvestigations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ErrorQuantity",
                table: "ErrorInvestigations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "InventoryBy",
                table: "ErrorInvestigations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "ErrorInvestigations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PositionCode",
                table: "ErrorInvestigations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "QuantityDifference",
                table: "ErrorInvestigations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalQuantity",
                table: "ErrorInvestigations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UnitPrice",
                table: "ErrorInvestigations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "WareHouseLocation",
                table: "ErrorInvestigations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
