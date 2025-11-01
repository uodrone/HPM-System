using HPM_System.EventService.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HPM_System.EventService.Controllers
{
    public interface IEventController
    {
        Task<ActionResult<EventModel>> CreateEventAsync([Description("Модель события для добавления"), FromBody] EventModel? eventModel, CancellationToken ct);
        Task<IActionResult> DeleteEventAsync(int id, CancellationToken ct);
        Task<ActionResult<IEnumerable<EventModel>>> GetAllEventsAsync(CancellationToken ct);
        Task<ActionResult<IEnumerable<EventModel>>> GetAllUserEventsAsync(long userId, CancellationToken ct);
        Task<ActionResult<EventModel>> GetEventByIdAsync(long id, CancellationToken ct);
        Task<IActionResult> UpdateEventAsync([Description("Модель события для обновления"), FromBody] EventModel updatedEvent, CancellationToken ct);
    }
}