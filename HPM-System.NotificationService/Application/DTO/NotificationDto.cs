using HPM_System.NotificationService.Domain.ValueObjects;

namespace HPM_System.NotificationService.Application.DTO
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public NotificationType Type { get; set; }
        public bool IsReadable { get; set; }
        public List<NotificationUserDto> Recipients { get; set; } = new();
    }
}
