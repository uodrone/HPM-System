using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HPMSystem.ApartmentService.Migrations
{
    /// <inheritdoc />
    public partial class AddNewModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.AddColumn<decimal>(
                name: "Share",
                table: "ApartmentUsers",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "HouseId",
                table: "Apartment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApartmentUserStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApartmentId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApartmentUserStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApartmentUserStatuses_ApartmentUsers_ApartmentId_UserId",
                        columns: x => new { x.ApartmentId, x.UserId },
                        principalTable: "ApartmentUsers",
                        principalColumns: new[] { "ApartmentId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApartmentUserStatuses_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Владелец" },
                    { 2, "Жилец" },
                    { 3, "Прописан" },
                    { 4, "Временно проживающий" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApartmentUserStatuses_ApartmentId_UserId_StatusId",
                table: "ApartmentUserStatuses",
                columns: new[] { "ApartmentId", "UserId", "StatusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApartmentUserStatuses_StatusId",
                table: "ApartmentUserStatuses",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Statuses_Name",
                table: "Statuses",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApartmentUserStatuses");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropColumn(
                name: "Share",
                table: "ApartmentUsers");

            migrationBuilder.DropColumn(
                name: "HouseId",
                table: "Apartment");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
