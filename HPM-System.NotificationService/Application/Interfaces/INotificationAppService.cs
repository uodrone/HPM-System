using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Domain.Entities;

namespace HPM_System.NotificationService.Application.Interfaces
{
    public interface INotificationAppService
    {
        Task<IEnumerable<NotificationDto>> GetAllAsync();
        Task<NotificationDto?> GetByIDAsync(Guid id);
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDTO dto);
        Task<bool> MarkAsRead(Guid recipientId);
    }
}