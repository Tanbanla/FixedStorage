#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class remove_shelf_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactoryName",
                table: "Storages");

            migrationBuilder.RenameColumn(
                name: "ShelfName",
                table: "Positions",
                newName: "SepcialRequire");

            migrationBuilder.AddColumn<string>(
                name: "FactoryName",
                table: "Positions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactoryName",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "SepcialRequire",
                table: "Positions",
                newName: "ShelfName");

            migrationBuilder.AddColumn<string>(
                name: "FactoryName",
                table: "Storages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
