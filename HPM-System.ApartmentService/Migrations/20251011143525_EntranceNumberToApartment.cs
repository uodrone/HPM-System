using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPMSystem.ApartmentService.Migrations
{
    /// <inheritdoc />
    public partial class EntranceNumberToApartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntranceNumber",
                table: "Apartment",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntranceNumber",
                table: "Apartment");
        }
    }
}
