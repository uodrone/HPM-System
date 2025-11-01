using HPM_System.EventService.Models;
using HPM_System.EventService.Repositories;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HPM_System.EventService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase, IEventController
    {
        
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;

        public EventController(ILogger<EventController> logger, IEventService eventService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        /// <summary>
        /// Создать новое событие
        /// </summary>
        [HttpPost]
        [EndpointDescription("Создать новое событие")]
        public async Task<ActionResult<EventModel>> CreateEventAsync([Description("Модель события для добавления")][FromBody] EventModel? eventModel, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result =  await _eventService.CreateEventAsync(eventModel, ct);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Получить все события
        /// </summary>
        [HttpGet]
        [EndpointDescription("Получить все события")]
        public async Task<ActionResult<IEnumerable<EventModel>>> GetAllEventsAsync(CancellationToken ct)
        {
            try
            {
                return Ok(await _eventService.GetAllEventsAsync(ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Получить событие по ID
        /// </summary>
        [HttpGet("{id}")]
        [EndpointDescription("Получить событие по ИД")]
        public async Task<ActionResult<EventModel>> GetEventByIdAsync(long id, CancellationToken ct)
        {
            try
            {
                var eventModel = await _eventService.GetEventByIdAsync(id, ct);

                if (eventModel == null)
                {
                    return NotFound($"Событие с ID {id} не найдена");
                }

                return Ok(eventModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Получить события пользователя
        /// </summary>
        [HttpGet("{userId}")]
        [EndpointDescription("Получить события пользователя")]
        public async Task<ActionResult<IEnumerable<EventModel>>> GetAllUserEventsAsync(long userId, CancellationToken ct)
        {
            try
            {
                return Ok(await _eventService.GetAllUserEventsAsync(userId, ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        [HttpPut("{id}")]
        [EndpointDescription("Обновить событие")]
        public async Task<IActionResult> UpdateEventAsync([Description("Модель события для обновления")][FromBody] EventModel updatedEvent, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                await _eventService.UpdateEventAsync(updatedEvent, ct);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }

        /// <summary>
        /// Удалить событие
        /// </summary>
        [HttpDelete("{id}")]
        [EndpointDescription("Удалить событие")]
        public async Task<IActionResult> DeleteEventAsync(int id, CancellationToken ct)
        {
            try
            {
                var eventToRemove = await _eventService.GetEventByIdAsync(id, ct);

                if (eventToRemove == null)
                {
                    return NotFound($"Событие с ID {id} не найдено");
                }

                await _eventService.DeleteEventAsync(id, ct);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }
    }
}

