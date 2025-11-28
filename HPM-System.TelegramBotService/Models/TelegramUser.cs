namespace HPM_System.TelegramBotService.Models
{
    public class TelegramUser
    {
        public int Id { get; set; } // Первичный ключ
        public Guid UserId { get; set; } // ID из системы
        public long TelegramChatId { get; set; } // ID чата с ботом
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}