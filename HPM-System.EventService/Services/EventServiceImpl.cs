// HPM_System.EventService.Services/EventServiceImpl.cs
using HPM_System.EventService.DTOs;
using HPM_System.EventService.Interfaces;
using HPM_System.EventService.Models;
using HPM_System.EventService.Repositories;
using HPM_System.EventService.Services.HttpClients;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HPM_System.EventService.Services
{
    public class EventServiceImpl : IEventService
    {
        private readonly IEventRepository _eventRepo;
        private readonly IEventParticipantRepository _participantRepo;
        private readonly IApartmentServiceClient _apartmentService;
        private readonly INotificationServiceClient _notificationService;
        private readonly ILogger<EventServiceImpl> _logger;

        public EventServiceImpl(
            IEventRepository eventRepo,
            IEventParticipantRepository participantRepo,
            IApartmentServiceClient apartmentService,
            INotificationServiceClient notificationService,
            ILogger<EventServiceImpl> logger)
        {
            _eventRepo = eventRepo;
            _participantRepo = participantRepo;
            _apartmentService = apartmentService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<EventDto> CreateEventAsync(CreateEventRequest request, Guid initiatorUserId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Заголовок события обязателен.", nameof(request.Title));

            // Создаём событие
            var newEvent = new Event
            {
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                EventDateTime = request.EventDateTime,
                Place = request.Place,
                CreatedAt = DateTime.UtcNow
            };

            await _eventRepo.AddAsync(newEvent, ct);

            // Определяем получателей
            List<Guid> targetUserIds = new();

            if (request.CommunityId.HasValue)
            {
                if (request.CommunityType == CommunityType.House)
                {
                    targetUserIds = await _apartmentService.GetHouseUserIdsAsync(request.CommunityId.Value, ct);
                }
                else
                {
                    throw new ArgumentException($"Тип сообщества {request.CommunityType} не поддерживается.");
                }
                targetUserIds ??= new();
            }
            else
            {
                targetUserIds.Add(initiatorUserId); // приватное событие
            }

            if (!targetUserIds.Any())
            {
                _logger.LogWarning("Событие ID={EventId} создано без получателей", newEvent.Id);
            }

            // Создаём участников
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
                await _participantRepo.AddRangeAsync(participants, ct);
                await _participantRepo.SaveChangesAsync(ct);
            }

            // Отправляем уведомление о создании события
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
            var ev = await _eventRepo.GetByIdAsync(eventId, ct);
            if (ev == null) return null;

            var subscribedCount = await _eventRepo.GetSubscribedCountAsync(eventId, ct);

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
            var eventIds = await _participantRepo.GetEventIdsForUserAsync(userId, ct);
            if (!eventIds.Any()) return new();

            var events = await _eventRepo.GetByIdsAsync(eventIds, ct);
            var result = new List<EventDto>();

            foreach (var ev in events)
            {
                var isSubscribed = await _participantRepo.IsUserSubscribedAsync(ev.Id, userId, ct);
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

        public async Task<bool> IsUserParticipantAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            return await _participantRepo.IsUserParticipantAsync(eventId, userId, ct);
        }

        public async Task SubscribeAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            var participant = await _participantRepo.GetAsync(eventId, userId, ct);
            if (participant == null)
                throw new InvalidOperationException("Пользователь не является получателем этого события");

            if (participant.IsSubscribed) return;

            participant.IsSubscribed = true;
            participant.SubscribedAt = DateTime.UtcNow;

            await _participantRepo.SaveChangesAsync(ct);
        }

        public async Task UnsubscribeAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            var participant = await _participantRepo.GetAsync(eventId, userId, ct);
            if (participant == null || !participant.IsSubscribed) return;

            participant.IsSubscribed = false;
            participant.SubscribedAt = null;

            await _participantRepo.SaveChangesAsync(ct);
        }

        public async Task<bool> IsUserSubscribedAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            return await _participantRepo.IsUserSubscribedAsync(eventId, userId, ct);
        }
    }
}