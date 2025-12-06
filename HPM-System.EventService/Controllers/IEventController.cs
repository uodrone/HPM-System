using HPM_System.EventService.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HPM_System.EventService.Controllers
{
    public interface IEventController
    {
        Task<ActionResult<EventModel>> CreateEventAsync([Description("Модель события для добавления"), FromBody] EventModelWithImageRequest? eventModel,  CancellationToken ct);
        Task<IActionResult> DeleteEventAsync(int id, CancellationToken ct);
        Task<ActionResult<IEnumerable<EventModelWithImageRequest>>> GetAllEventsAsync(CancellationToken ct);
        Task<ActionResult<IEnumerable<EventModelWithImageRequest>>> GetAllUserEventsAsync(Guid userId, CancellationToken ct);
        Task<ActionResult<EventModelWithImageRequest>> GetEventByIdAsync(long id, CancellationToken ct);
        Task<IActionResult> UpdateEventAsync([Description("Модель события для обновления"), FromBody] EventModelWithImageRequest updatedEvent, CancellationToken ct);
    }
}