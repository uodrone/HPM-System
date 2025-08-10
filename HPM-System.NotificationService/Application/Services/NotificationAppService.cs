using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HPM_System.NotificationService.Application.Services
{
    public class NotificationAppService : INotificationAppService
    {
        private readonly INotificationRepository _repository;

        public NotificationAppService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Notification> CreateNotificationAsync(CreateNotificationDTO dto)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                ImageUrl = dto.ImageUrl,
                CreatedBy = dto.CreatedBy,
                Message = dto.Message,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow,
                Recipients = dto.UserIdList.Select(userId => new NotificationUsers
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                }).ToList()
            };

            return await _repository.AddAsync(notification);            
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Notification?> GetByIDAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> MarkAsRead(Guid id)
        {           
            return await _repository.MarkAsReadAsync(id);
        }
    }
}
