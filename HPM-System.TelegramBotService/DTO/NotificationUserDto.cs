namespace HPM_System.TelegramBotService.DTO
{
    public class NotificationUserDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
