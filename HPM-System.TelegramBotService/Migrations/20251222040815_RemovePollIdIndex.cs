using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPM_System.TelegramBotService.Migrations
{
    /// <inheritdoc />
    public partial class RemovePollIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelegramPolls_PollId",
                table: "TelegramPolls");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TelegramPolls_PollId",
                table: "TelegramPolls",
                column: "PollId",
                unique: true);
        }
    }
}
