namespace HPM_System.NotificationService.Domain.Entities
{
    public class NotificationUsers
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public Notification Notification { get; set; }
    }
}
