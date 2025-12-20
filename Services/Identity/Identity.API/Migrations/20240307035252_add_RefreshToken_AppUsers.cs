using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Identity.API.Migrations
{
    /// <inheritdoc />
    public partial class add_RefreshToken_AppUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AppUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AppUsers");
        }
    }
}
