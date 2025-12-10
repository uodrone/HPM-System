using HPM_System.EventService.DTOs;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HPM_System.EventService.Helpers;

namespace HPM_System.EventService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            var dto = await _eventService.CreateEventAsync(request, userId, ct);
            return CreatedAtAction(nameof(GetEventById), new { id = dto.Id }, dto);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<EventDto>> GetEventById(long id, CancellationToken ct = default)
        {
            var dto = await _eventService.GetEventByIdAsync(id, ct);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetUserEvents(CancellationToken ct = default)
        {
            var userId = User.GetUserId();
            var events = await _eventService.GetAllEventsForUserAsync(userId, ct);
            return Ok(events);
        }

        [HttpPost("{id:long}/subscribe")]
        public async Task<IActionResult> Subscribe(long id, CancellationToken ct = default)
        {
            var userId = User.GetUserId();
            await _eventService.SubscribeAsync(id, userId, ct);
            return Ok();
        }

        [HttpDelete("{id:long}/unsubscribe")]
        public async Task<IActionResult> Unsubscribe(long id, CancellationToken ct = default)
        {
            var userId = User.GetUserId();
            await _eventService.UnsubscribeAsync(id, userId, ct);
            return Ok();
        }
    }
}