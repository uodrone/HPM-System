using HPM_System.NotificationService.Domain.ValueObjects;

namespace HPM_System.NotificationService.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }
        public NotificationType Type { get; set; }
        public bool IsReadable { get; set; } = true;

        public List<NotificationUsers> Recipients { get; set; } = new List<NotificationUsers>();

    }
}
