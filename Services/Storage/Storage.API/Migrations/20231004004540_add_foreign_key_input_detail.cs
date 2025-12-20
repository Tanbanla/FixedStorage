using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class add_foreign_key_input_detail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "InputId",
                table: "InputDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InputDetails_InputId",
                table: "InputDetails",
                column: "InputId",
                unique: true,
                filter: "[InputId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InputDetails_InputFromBwins_InputId",
                table: "InputDetails",
                column: "InputId",
                principalTable: "InputFromBwins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InputDetails_InputFromBwins_InputId",
                table: "InputDetails");

            migrationBuilder.DropIndex(
                name: "IX_InputDetails_InputId",
                table: "InputDetails");

            migrationBuilder.AlterColumn<string>(
                name: "InputId",
                table: "InputDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
