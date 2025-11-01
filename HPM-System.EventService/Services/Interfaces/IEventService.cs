using HPM_System.EventService.Models;
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.EventService.Services.Interfaces
{
    public interface IEventService
    {
        Task<ActionResult<EventModel>> CreateEventAsync(EventModel eventModel, CancellationToken ct);
        Task DeleteEventAsync(EventModel model, CancellationToken ct);
        Task<ActionResult<IEnumerable<EventModel>>> GetAllEventsAsync(CancellationToken ct);
        Task<ActionResult<IEnumerable<EventModel>>> GetAllUserEventsAsync(Guid userId, CancellationToken ct);
        Task<EventModel?> GetEventByIdAsync(long id, CancellationToken ct);
        Task UpdateEventAsync(EventModel updatedEvent, CancellationToken ct);
    }
}
