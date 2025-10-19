using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HPMSystem.ApartmentService.Migrations
{
    /// <inheritdoc />
    public partial class AddHouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "HouseId",
                table: "Apartment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "House",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    City = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Entrances = table.Column<int>(type: "integer", nullable: false),
                    Floors = table.Column<int>(type: "integer", nullable: false),
                    HasGas = table.Column<bool>(type: "boolean", nullable: false),
                    HasElectricity = table.Column<bool>(type: "boolean", nullable: false),
                    HasElevator = table.Column<bool>(type: "boolean", nullable: false),
                    HeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostIndex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApartmentsArea = table.Column<double>(type: "double precision", nullable: true),
                    TotalArea = table.Column<double>(type: "double precision", nullable: true),
                    LandArea = table.Column<double>(type: "double precision", nullable: true),
                    IsApartmentBuilding = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_House", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    HouseId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => x.Id);
                    table.ForeignKey(
                        name: "FK_District_District_ParentId",
                        column: x => x.ParentId,
                        principalTable: "District",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_District_House_HouseId",
                        column: x => x.HouseId,
                        principalTable: "House",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apartment_HouseId",
                table: "Apartment",
                column: "HouseId");

            migrationBuilder.CreateIndex(
                name: "IX_District_HouseId",
                table: "District",
                column: "HouseId");

            migrationBuilder.CreateIndex(
                name: "IX_District_ParentId",
                table: "District",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_House_City_Street_Number",
                table: "House",
                columns: new[] { "City", "Street", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_House_HeadId",
                table: "House",
                column: "HeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Apartment_House_HouseId",
                table: "Apartment",
                column: "HouseId",
                principalTable: "House",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartment_House_HouseId",
                table: "Apartment");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "House");

            migrationBuilder.DropIndex(
                name: "IX_Apartment_HouseId",
                table: "Apartment");

            migrationBuilder.AlterColumn<int>(
                name: "HouseId",
                table: "Apartment",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
