using HPM_System.EventService.Models;
using HPM_System.EventService.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HPM_System.EventService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventModelRepository _eventRepository;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventModelRepository repository, ILogger<EventController> logger)
        {
            _eventRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Необходимо создать сервис ЕвентСервис. В нем инкапсулировать логику всю. В том числе, при инициализации этого сервиса
            // необходимо кэшировать все записи событий, ибо нечего ходить в БД постоянно
        }

        /// <summary>
        /// Создать новое событие
        /// </summary>
        [HttpPost]
        [EndpointDescription("Создать новое событие")]
        public async Task<ActionResult<EventModel>> CreateEventAsync([Description("Модель события для добавления")] [FromBody] EventModel? eventModel, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Random r = new Random(10);
                var next = r.Next(4);

                var a = new EventModel();
                a.Place = $"Место проведения {next}";
                a.EventName = $"Название какото то события {next}";
                a.EventDescription = $"Описание какого то события {next}";
                a.EventDateTime = DateTime.UtcNow;
                a.HouseId = 1L;
                a.UserId = 15L;
                a.ImageIds = new List<long>() 
                {
                    20L,25L,30L
                };


                var result = await _eventRepository.AddAsync(a, ct);

                return Ok(a);
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
                return Ok(await _eventRepository.GetAllAsync(ct));
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
                var eventModel = await _eventRepository.GetByIdAsync(id, ct);

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
                var eventModels = await _eventRepository.GetAllUserEventsAsync(userId, ct);

                if (!eventModels.Any())
                {
                    return NotFound($"События пользователя с ID {userId} не найдены");
                }

                return Ok(eventModels);
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

                await _eventRepository.UpdateAsync(updatedEvent, ct);
                return NoContent();
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
                var eventToRemove = await _eventRepository.GetByIdAsync(id, ct);

                if (eventToRemove == null)
                {
                    return NotFound($"Событие с ID {id} не найдено");
                }

                await _eventRepository.DeleteAsync(eventToRemove, ct);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, $"Внутренняя ошибка сервера {nameof(EventController)}");
            }
        }
    }
}

