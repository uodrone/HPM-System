using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPMSystem.ApartmentService.Migrations
{
    /// <inheritdoc />
    public partial class FixApartmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Apartment_CommunityId",
                table: "Apartment");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Apartment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommunityId",
                table: "Apartment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Apartment_CommunityId",
                table: "Apartment",
                column: "CommunityId");
        }
    }
}
