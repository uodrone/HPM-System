using HPM_System.NotificationService.Application.DTO;

public interface INotificationAppService
{
    Task<IEnumerable<NotificationDto>> GetAllAsync();
    Task<NotificationDto?> GetByIDAsync(Guid id);
    Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDTO dto);
    Task<bool> MarkAsRead(Guid recipientId);
    Task<bool> MarkAsReadByIdsAsync(Guid notificationId, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
}