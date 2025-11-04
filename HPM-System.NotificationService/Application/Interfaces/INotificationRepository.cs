using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Domain.Entities;

namespace HPM_System.NotificationService.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<IEnumerable<Notification>> GetAllAsync(bool notReadOnly = false);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid recipientId);
    }
}
