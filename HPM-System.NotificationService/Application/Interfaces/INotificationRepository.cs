using HPM_System.NotificationService.Domain.Entities;

namespace HPM_System.NotificationService.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllAsync(bool notReadOnly = false);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> AddAsync(Notification notification);
        Task<bool> MarkAsReadAsync(Guid id);
    }
}
