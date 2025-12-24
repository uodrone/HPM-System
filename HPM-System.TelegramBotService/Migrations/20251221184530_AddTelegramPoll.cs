using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HPM_System.TelegramBotService.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramPoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramPolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VotingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApartmentId = table.Column<long>(type: "bigint", nullable: false),
                    PollId = table.Column<string>(type: "text", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAnswered = table.Column<bool>(type: "boolean", nullable: false),
                    SelectedOption = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramPolls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPolls_PollId",
                table: "TelegramPolls",
                column: "PollId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPolls_VotingId",
                table: "TelegramPolls",
                column: "VotingId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPolls_VotingId_UserId_ApartmentId",
                table: "TelegramPolls",
                columns: new[] { "VotingId", "UserId", "ApartmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramPolls");
        }
    }
}
