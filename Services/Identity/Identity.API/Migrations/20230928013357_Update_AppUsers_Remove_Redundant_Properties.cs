using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Identity.API.Migrations
{
    /// <inheritdoc />
    public partial class Update_AppUsers_Remove_Redundant_Properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiredDateLockedByExpiredPassword",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ExpiredDateLockedByUnactive",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "IsLockedByExpiredPassword",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "IsLockedByUnactive",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "LockCause",
                table: "AppUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredDateLockedByExpiredPassword",
                table: "AppUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredDateLockedByUnactive",
                table: "AppUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLockedByExpiredPassword",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLockedByUnactive",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LockCause",
                table: "AppUsers",
                type: "int",
                nullable: true);
        }
    }
}
