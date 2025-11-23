using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HPM_System.EventService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HouseId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Place = table.Column<string>(type: "text", nullable: true),
                    EventName = table.Column<string>(type: "text", nullable: true),
                    EventDescription = table.Column<string>(type: "text", nullable: true),
                    ImageIds = table.Column<long[]>(type: "bigint[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ImageId);
                });

            // Вставка тестовых данных в Events
            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "HouseId", "UserId", "EventDateTime", "Place", "EventName", "EventDescription" },
                values: new object[,]
                {
                { 1L, 1001L, Guid.Parse("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 11, 20, 10, 0, 0, DateTimeKind.Utc), "House 1001", "Party", "Birthday party of John" },
                { 2L, 1002L, Guid.Parse("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 11, 21, 12, 0, 0, DateTimeKind.Utc), "House 1002", "Meeting", "Business meeting" },
                { 3L, 1003L, Guid.Parse("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 11, 22, 15, 0, 0, DateTimeKind.Utc), "House 1003", "Conference", "Tech conference" },
                { 4L, 1004L, Guid.Parse("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 11, 23, 9, 0, 0, DateTimeKind.Utc), "House 1004", "Workshop", "Skill workshop" },
                { 5L, 1005L, Guid.Parse("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 11, 24, 14, 0, 0, DateTimeKind.Utc), "House 1005", "Concert", "Live music concert" },
                { 6L, 1006L, Guid.Parse("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 11, 25, 18, 30, 0, DateTimeKind.Utc), "House 1006", "Launch", "Product launch event" },
                { 7L, null, Guid.Parse("77777777-7777-7777-7777-777777777777"), new DateTime(2025, 11, 26, 11, 30, 0, DateTimeKind.Utc), "Open Area", "Exhibition", "Art exhibition" },
                { 8L, null, Guid.Parse("88888888-8888-8888-8888-888888888888"), new DateTime(2025, 11, 27, 16, 0, 0, DateTimeKind.Utc), "City Hall", "Ceremony", "Award ceremony" },
                { 9L, 1009L, Guid.Parse("99999999-9999-9999-9999-999999999999"), new DateTime(2025, 11, 28, 13, 0, 0, DateTimeKind.Utc), "House 1009", "Seminar", "Health seminar" },
                { 10L, 1010L, Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"), new DateTime(2025, 11, 29, 17, 0, 0, DateTimeKind.Utc), "House 1010", "Networking", "Business networking event" }
                });

            // Вставка тестовых данных в Images
            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "ImageId", "EventId" },
                values: new object[,]
                {
                { 1, 1L },
                { 2, 1L},
                { 3, 2L},
                { 4, 3L },
                { 5, 4L },
                { 6, 5L },
                { 7, 6L },
                { 8, 7L },
                { 9, 8L },
                { 10, 10L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Images");
        }
    }
}
