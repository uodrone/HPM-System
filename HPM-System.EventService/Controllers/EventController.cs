using HPM_System.EventService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HPM_System.EventService.DTOs;

namespace HPM_System.EventService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventService eventService, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserIdFromClaims();
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
            var userId = GetUserIdFromClaims();
            var events = await _eventService.GetAllEventsForUserAsync(userId, ct);
            return Ok(events);
        }

        [HttpPost("{id:long}/subscribe")]
        public async Task<IActionResult> Subscribe(long id, CancellationToken ct = default)
        {
            var userId = GetUserIdFromClaims();
            await _eventService.SubscribeAsync(id, userId, ct);
            return Ok();
        }

        [HttpDelete("{id:long}/unsubscribe")]
        public async Task<IActionResult> Unsubscribe(long id, CancellationToken ct = default)
        {
            var userId = GetUserIdFromClaims();
            await _eventService.UnsubscribeAsync(id, userId, ct);
            return Ok();
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdStr = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            return userId;
        }
    }
}