using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Identity.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_LockSetting_AppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LockActSetting",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LockActTime",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LockPwTime",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LockPwdSetting",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockActSetting",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "LockActTime",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "LockPwTime",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "LockPwdSetting",
                table: "AppUsers");
        }
    }
}
