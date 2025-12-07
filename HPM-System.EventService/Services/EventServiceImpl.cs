using HPM_System.EventService.DataContext;
using HPM_System.EventService.DTOs;
using HPM_System.EventService.Interfaces;
using HPM_System.EventService.Models;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.EventService.Services
{
    public class EventServiceImpl : IEventService
    {
        private readonly AppDbContext _dbContext;
        private readonly IApartmentServiceClient _apartmentService;
        private readonly INotificationServiceClient _notificationService;
        private readonly ILogger<EventServiceImpl> _logger;

        public EventServiceImpl(
            AppDbContext dbContext,
            IApartmentServiceClient apartmentService,
            INotificationServiceClient notificationService,
            ILogger<EventServiceImpl> logger)
        {
            _dbContext = dbContext;
            _apartmentService = apartmentService;
            _notificationService = notificationService;
            _logger = logger;
        }

        // HPM_System.EventService.Services/EventService.cs
        public async Task<EventDto> CreateEventAsync(CreateEventRequest request, Guid initiatorUserId, CancellationToken ct = default)
        {
            // === Валидация ===
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Заголовок события обязателен.", nameof(request.Title));

            // === Создаём событие ===
            var newEvent = new Event
            {
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                EventDateTime = request.EventDateTime,
                Place = request.Place,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Events.Add(newEvent);
            await _dbContext.SaveChangesAsync(ct);

            // Определяем целевую аудиторию
            List<Guid> targetUserIds = new();

            if (request.CommunityId.HasValue)
            {
                // Поддерживаем пока только House
                if (request.CommunityType == CommunityType.House)
                {
                    targetUserIds = await _apartmentService.GetHouseOwnerIdsAsync(request.CommunityId.Value, ct);
                }
                else
                {
                    // В будущем можно расширить, но сейчас — ошибка
                    throw new ArgumentException($"Тип сообщества {request.CommunityType} не поддерживается.");
                }
                targetUserIds ??= new();
            }
            else
            {
                // Приватное событие — только инициатор
                targetUserIds.Add(initiatorUserId);
            }

            if (!targetUserIds.Any())
            {
                _logger.LogWarning("Событие ID={EventId} создано без получателей", newEvent.Id);
            }

            // Создаём участников события
            var participants = targetUserIds.Select(userId => new EventParticipant
            {
                EventId = newEvent.Id,
                UserId = userId,
                IsSubscribed = false,
                InvitedAt = DateTime.UtcNow,
                InvitedBy = initiatorUserId
            }).ToList();

            if (participants.Any())
            {
                await _dbContext.EventParticipants.AddRangeAsync(participants, ct);
                await _dbContext.SaveChangesAsync(ct);
            }

            // Отправляем уведомление
            if (targetUserIds.Any())
            {
                var notification = new CreateEventNotificationRequest
                {
                    Title = $"Новое событие: {newEvent.Title}",
                    Message = $"{newEvent.Description ?? "Без описания"}\nДата: {newEvent.EventDateTime:dd.MM.yyyy HH:mm}",
                    ImageUrl = newEvent.ImageUrl,
                    CreatedBy = initiatorUserId,
                    IsReadable = true,
                    UserIdList = targetUserIds
                };

                await _notificationService.CreateAsync(notification, ct);
            }

            // Возвращаем DTO
            return new EventDto
            {
                Id = newEvent.Id,
                Title = newEvent.Title,
                Description = newEvent.Description,
                ImageUrl = newEvent.ImageUrl,
                EventDateTime = newEvent.EventDateTime,
                Place = newEvent.Place,
                CreatedAt = newEvent.CreatedAt,
                SubscribedCount = 0
            };
        }

        public async Task<EventDto?> GetEventByIdAsync(long eventId, CancellationToken ct = default)
        {
            var ev = await _dbContext.Events.FindAsync(new object[] { eventId }, ct);
            if (ev == null) return null;

            var subscribedCount = await _dbContext.EventParticipants
                .CountAsync(p => p.EventId == eventId && p.IsSubscribed, ct);

            return new EventDto
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                ImageUrl = ev.ImageUrl,
                EventDateTime = ev.EventDateTime,
                Place = ev.Place,
                CreatedAt = ev.CreatedAt,
                SubscribedCount = subscribedCount
            };
        }

        public async Task<List<EventDto>> GetAllEventsForUserAsync(Guid userId, CancellationToken ct = default)
        {
            var eventIds = await _dbContext.EventParticipants
                .Where(p => p.UserId == userId)
                .Select(p => p.EventId)
                .Distinct()
                .ToListAsync(ct);

            if (!eventIds.Any()) return new();

            var events = await _dbContext.Events
                .Where(e => eventIds.Contains(e.Id))
                .ToListAsync(ct);

            var result = new List<EventDto>();
            foreach (var ev in events)
            {
                var isSubscribed = await _dbContext.EventParticipants
                    .AnyAsync(p => p.EventId == ev.Id && p.UserId == userId && p.IsSubscribed, ct);

                result.Add(new EventDto
                {
                    Id = ev.Id,
                    Title = ev.Title,
                    Description = ev.Description,
                    ImageUrl = ev.ImageUrl,
                    EventDateTime = ev.EventDateTime,
                    Place = ev.Place,
                    CreatedAt = ev.CreatedAt,
                    IsSubscribed = isSubscribed
                });
            }

            return result;
        }

        public async Task SubscribeAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            var participant = await _dbContext.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, ct);

            if (participant == null)
                throw new InvalidOperationException("Пользователь не является получателем этого события");

            if (participant.IsSubscribed) return; // уже подписан

            participant.IsSubscribed = true;
            participant.SubscribedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task UnsubscribeAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            var participant = await _dbContext.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, ct);

            if (participant == null || !participant.IsSubscribed) return;

            participant.IsSubscribed = false;
            participant.SubscribedAt = null;

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<bool> IsUserSubscribedAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            return await _dbContext.EventParticipants
                .AnyAsync(p => p.EventId == eventId && p.UserId == userId && p.IsSubscribed, ct);
        }
    }
}