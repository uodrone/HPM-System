using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPM_System.EventService.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionEventsModelForExternalServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Events_EventDateTime",
                table: "Events",
                newName: "IX_Event_EventDateTime");

            migrationBuilder.AddColumn<long>(
                name: "EventId1",
                table: "EventParticipants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Reminder24hSent",
                table: "EventParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Reminder24hSentAt",
                table: "EventParticipants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Reminder2hSent",
                table: "EventParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Reminder2hSentAt",
                table: "EventParticipants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipant_24hReminder",
                table: "EventParticipants",
                columns: new[] { "EventId", "IsSubscribed", "Reminder24hSent" });

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipant_2hReminder",
                table: "EventParticipants",
                columns: new[] { "EventId", "IsSubscribed", "Reminder2hSent" });

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_EventId1",
                table: "EventParticipants",
                column: "EventId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EventParticipants_Events_EventId1",
                table: "EventParticipants",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventParticipants_Events_EventId1",
                table: "EventParticipants");

            migrationBuilder.DropIndex(
                name: "IX_EventParticipant_24hReminder",
                table: "EventParticipants");

            migrationBuilder.DropIndex(
                name: "IX_EventParticipant_2hReminder",
                table: "EventParticipants");

            migrationBuilder.DropIndex(
                name: "IX_EventParticipants_EventId1",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "Reminder24hSent",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "Reminder24hSentAt",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "Reminder2hSent",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "Reminder2hSentAt",
                table: "EventParticipants");

            migrationBuilder.RenameIndex(
                name: "IX_Event_EventDateTime",
                table: "Events",
                newName: "IX_Events_EventDateTime");
        }
    }
}
