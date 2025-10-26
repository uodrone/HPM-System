using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Domain.Entities;

namespace HPM_System.NotificationService.Application.Interfaces
{
    public interface INotificationAppService
    {
        public Task<IEnumerable<Notification>> GetAllAsync();
        public Task<Notification?> GetByIDAsync(Guid id);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        public Task<Notification> CreateNotificationAsync(CreateNotificationDTO dto);
        public Task<bool> MarkAsRead(Guid id);
    }
}
