// HPM_System.NotificationService.Application.Services/NotificationAppService.cs
using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Domain.Entities;
using System.Linq; // ⚠️ ОБЯЗАТЕЛЬНО

namespace HPM_System.NotificationService.Application.Services
{
    public class NotificationAppService : INotificationAppService
    {
        private readonly INotificationRepository _repository;

        public NotificationAppService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDTO dto)
        {
            if (dto.UserIdList == null || !dto.UserIdList.Any())
                throw new ArgumentException("UserIdList не может быть пустым.");

            // удаляем дубликаты — это решает ошибку 23505
            var uniqueUserIds = dto.UserIdList.Distinct().ToList();

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Message = dto.Message,
                ImageUrl = dto.ImageUrl,
                CreatedBy = dto.CreatedBy,
                Type = dto.Type,
                IsReadable = dto.IsReadable,
                CreatedAt = DateTime.UtcNow,
                Recipients = uniqueUserIds.Select(userId => new NotificationUsers
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                }).ToList()
            };

            var saved = await _repository.AddAsync(notification);
            return MapToDto(saved);
        }

        public async Task<IEnumerable<NotificationDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<NotificationDto?> GetByIDAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _repository.GetByUserIdAsync(userId);
            return entities.Select(MapToDto);
        }

        public async Task<bool> MarkAsRead(Guid recipientId)
        {
            return await _repository.MarkAsReadAsync(recipientId);
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(Guid userId)
        {
            var entities = await _repository.GetUnreadByUserIdAsync(userId);
            return entities.Select(MapToDto);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _repository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadByIdsAsync(Guid notificationId, Guid userId)
        {
            return await _repository.MarkAsReadByIdsAsync(notificationId, userId);
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            return await _repository.MarkAllAsReadAsync(userId);
        }

        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            ImageUrl = n.ImageUrl,
            CreatedAt = n.CreatedAt,
            CreatedBy = n.CreatedBy,
            Type = n.Type,
            IsReadable = n.IsReadable,
            Recipients = n.Recipients.Select(r => new NotificationUserDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ReadAt = r.ReadAt
            }).ToList()
        };
    }
}