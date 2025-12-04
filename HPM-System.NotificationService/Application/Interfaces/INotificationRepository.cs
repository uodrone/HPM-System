using HPM_System.NotificationService.Domain.Entities;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<IEnumerable<Notification>> GetAllAsync(bool notReadOnly = false);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid recipientId);
    Task<bool> MarkAsReadByIdsAsync(Guid notificationId, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
}