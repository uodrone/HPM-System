using HPM_System.NotificationService.Domain.ValueObjects;

namespace HPM_System.NotificationService.Application.DTO
{
    public class CreateNotificationDTO
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string CreatedBy { get; set; } = null!;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public bool IsReadable { get; set; } = true;
        public List<Guid> UserIdList { get; set; } = new();
    }
}
