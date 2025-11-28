namespace HPM_System.TelegramBotService.DTO
{
    public record NotificationPublishedEvent(
        Guid NotificationId,
        string Title,
        string Message,
        string? ImageUrl,
        DateTime CreatedAt,
        Guid CreatedBy,
        NotificationType Type,
        List<Guid> RecipientUserIds
    );
}
