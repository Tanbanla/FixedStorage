using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIVN.FixedStorage.Services.Storage.API.Migrations
{
    /// <inheritdoc />
    public partial class add_composition_index_positionhistories_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "PositionHistory_CreatedAt",
                table: "PositionHistories",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "PositionHistory_DepartmentId",
                table: "PositionHistories",
                column: "DepartmentId")
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "PositionHistoryType", "FactoryId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "PositionHistory_CreatedAt",
                table: "PositionHistories");

            migrationBuilder.DropIndex(
                name: "PositionHistory_DepartmentId",
                table: "PositionHistories");
        }
    }
}
