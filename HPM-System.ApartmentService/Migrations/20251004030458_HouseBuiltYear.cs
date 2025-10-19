using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPMSystem.ApartmentService.Migrations
{
    /// <inheritdoc />
    public partial class HouseBuiltYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "builtYear",
                table: "House",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "builtYear",
                table: "House");
        }
    }
}
