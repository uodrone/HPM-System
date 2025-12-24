namespace HPM_System.TelegramBotService.Models
{
    public class TelegramPoll
    {
        public int Id { get; set; }
        public Guid VotingId { get; set; } // ID голосования из VotingService
        public Guid UserId { get; set; } // Пользователь, которому отправлен poll
        public long ApartmentId { get; set; } // Квартира пользователя
        public string PollId { get; set; } = null!; // ID poll от Telegram
        public long ChatId { get; set; } // ChatId пользователя
        public int MessageId { get; set; } // ID сообщения с poll
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsAnswered { get; set; } // Ответил ли пользователь
        public string? SelectedOption { get; set; } // Выбранный вариант
    }
}