// HPM_System.EventService.Services.Interfaces/IEventService.cs
using HPM_System.EventService.Models;
using HPM_System.EventService.DTOs;

namespace HPM_System.EventService.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventDto> CreateEventAsync(CreateEventRequest request, Guid initiatorUserId, CancellationToken ct = default);
        Task<EventDto?> GetEventByIdAsync(long eventId, CancellationToken ct = default);
        Task<List<EventDto>> GetAllEventsForUserAsync(Guid userId, CancellationToken ct = default);
        Task SubscribeAsync(long eventId, Guid userId, CancellationToken ct = default);
        Task UnsubscribeAsync(long eventId, Guid userId, CancellationToken ct = default);
        Task<bool> IsUserSubscribedAsync(long eventId, Guid userId, CancellationToken ct = default);
        Task<bool> IsUserParticipantAsync(long eventId, Guid userId, CancellationToken ct = default);
    }
}