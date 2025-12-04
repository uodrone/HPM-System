using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPM_System.NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForNotificationUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_NotificationUsers_NotificationId_UserId",
                table: "NotificationUsers",
                newName: "IX_NotificationUsers_NotificationId_UserId_Unique");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUsers_NotificationId",
                table: "NotificationUsers",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUsers_ReadAt",
                table: "NotificationUsers",
                column: "ReadAt",
                filter: "\"ReadAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUsers_UserId_ReadAt",
                table: "NotificationUsers",
                columns: new[] { "UserId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUsers_UserId_ReadAt_NotificationId",
                table: "NotificationUsers",
                columns: new[] { "UserId", "ReadAt", "NotificationId" },
                filter: "\"ReadAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedBy",
                table: "Notifications",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type_CreatedAt",
                table: "Notifications",
                columns: new[] { "Type", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationUsers_NotificationId",
                table: "NotificationUsers");

            migrationBuilder.DropIndex(
                name: "IX_NotificationUsers_ReadAt",
                table: "NotificationUsers");

            migrationBuilder.DropIndex(
                name: "IX_NotificationUsers_UserId_ReadAt",
                table: "NotificationUsers");

            migrationBuilder.DropIndex(
                name: "IX_NotificationUsers_UserId_ReadAt_NotificationId",
                table: "NotificationUsers");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedBy",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Type_CreatedAt",
                table: "Notifications");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationUsers_NotificationId_UserId_Unique",
                table: "NotificationUsers",
                newName: "IX_NotificationUsers_NotificationId_UserId");
        }
    }
}
