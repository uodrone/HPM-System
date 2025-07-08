using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HPM_System.ApartmentService.Data;
using HPM_System.ApartmentService.Models;
using Microsoft.Extensions.Logging;

namespace HPM_System.ApartmentService.Controllers
{
    /// <summary>
    /// Контроллер для управления квартирами (CRUD операции).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ApartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApartmentsController> _logger;

        /// <summary>
        /// Конструктор контроллера. Инициализирует контекст БД и логгер.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        /// <param name="logger">Интерфейс для логирования.</param>
        public ApartmentsController(AppDbContext context, ILogger<ApartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Apartments
        /// <summary>
        /// Получает список всех квартир.
        /// </summary>
        /// <returns>Список квартир.</returns>
        [HttpGet]
        public async Task<IActionResult> GetApartments()
        {
            try
            {
                _logger.LogInformation("Получение списка всех квартир.");

                var apartments = await _context.Apartment.ToListAsync();

                _logger.LogInformation("Успешно получено {Count} квартир.", apartments.Count);

                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка квартир.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла ошибка при получении данных.");
            }
        }

        // GET: api/Apartments/5
        /// <summary>
        /// Получает квартиру по ID.
        /// </summary>
        /// <param name="id">ID квартиры.</param>
        /// <returns>Квартира или 404 Not Found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApartment(int id)
        {
            try
            {
                _logger.LogInformation("Получение квартиры с ID: {Id}", id);

                var apartment = await _context.Apartment.FindAsync(id);

                if (apartment == null)
                {
                    _logger.LogWarning("Квартира с ID: {Id} не найдена.", id);
                    return NotFound();
                }

                _logger.LogInformation("Квартира с ID: {Id} успешно найдена.", id);
                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении квартиры с ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла ошибка при получении данных.");
            }
        }

        // POST: api/Apartments
        /// <summary>
        /// Создает новую квартиру.
        /// </summary>
        /// <param name="apartment">Модель квартиры.</param>
        /// <returns>Созданную квартиру и ссылку на нее.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateApartment(Apartment apartment)
        {
            try
            {
                _logger.LogInformation("Создание новой квартиры.");

                _context.Apartment.Add(apartment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Квартира создана успешно. ID: {Id}", apartment.Id);

                return CreatedAtAction(
                    nameof(GetApartment),
                    new { id = apartment.Id },
                    apartment
                );
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка сохранения квартиры в базе данных.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ошибка при сохранении данных в БД.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при создании квартиры.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла критическая ошибка.");
            }
        }

        // PUT: api/Apartments/5
        /// <summary>
        /// Обновляет существующую квартиру.
        /// </summary>
        /// <param name="id">ID квартиры для обновления.</param>
        /// <param name="apartment">Обновленные данные.</param>
        /// <returns>204 No Content при успехе.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApartment(int id, Apartment apartment)
        {
            if (id != apartment.Id)
            {
                _logger.LogWarning("Несовпадение ID в запросе и теле модели: {RequestedId} vs {ModelId}", id, apartment.Id);
                return BadRequest("ID в URL и в теле запроса не совпадают.");
            }

            try
            {
                _logger.LogInformation("Обновление квартиры с ID: {Id}", id);

                var existingApartment = await _context.Apartment.FindAsync(id);
                if (existingApartment == null)
                {
                    _logger.LogWarning("Попытка обновить несуществующую квартиру с ID: {Id}", id);
                    return NotFound();
                }

                // Обновляем поля
                existingApartment.Number = apartment.Number;
                existingApartment.NumbersOfRooms = apartment.NumbersOfRooms;
                existingApartment.ResidentialArea = apartment.ResidentialArea;
                existingApartment.TotalArea = apartment.TotalArea;
                existingApartment.Floor = apartment.Floor;
                existingApartment.UserId = apartment.UserId;

                _context.Entry(existingApartment).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Квартира с ID: {Id} успешно обновлена.", id);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException conEx)
            {
                if (!await ApartmentExists(id))
                {
                    _logger.LogWarning("Попытка обновить несуществующую квартиру с ID: {Id}", id);
                    return NotFound();
                }

                _logger.LogError(conEx, "Ошибка параллелизма при обновлении квартиры с ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ошибка параллелизма при обновлении данных.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении квартиры с ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла ошибка при обновлении данных.");
            }
        }

        // DELETE: api/Apartments/5
        /// <summary>
        /// Удаляет квартиру по ID.
        /// </summary>
        /// <param name="id">ID квартиры.</param>
        /// <returns>204 No Content при успехе.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApartment(int id)
        {
            try
            {
                _logger.LogInformation("Удаление квартиры с ID: {Id}", id);

                var apartment = await _context.Apartment.FindAsync(id);
                if (apartment == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующую квартиру с ID: {Id}", id);
                    return NotFound();
                }

                _context.Apartment.Remove(apartment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Квартира с ID: {Id} успешно удалена.", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении квартиры с ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла ошибка при удалении данных.");
            }
        }

        // GET: api/Apartments/by-user-id?userId=123
        /// <summary>
        /// Получает список квартир, связанных с указанным UserId.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список квартир или ошибку</returns>
        [HttpGet("by-user-id")]
        public async Task<IActionResult> GetApartmentsByUserId(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Получен недопустимый UserId: {UserId}", userId);
                return BadRequest("UserId должен быть положительным числом.");
            }

            try
            {
                _logger.LogInformation("Поиск квартир для пользователя с ID: {UserId}", userId);

                var apartments = await _context.Apartment
                    .Where(a => a.UserId.Contains(userId))
                    .ToListAsync();

                if (!apartments.Any())
                {
                    _logger.LogInformation("Для пользователя с ID: {UserId} квартиры не найдены.", userId);
                    return NotFound($"Квартиры для пользователя с ID {userId} не найдены.");
                }

                _logger.LogInformation("Найдено {Count} квартир для пользователя с ID: {UserId}", apartments.Count, userId);
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении квартир для пользователя с ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Произошла ошибка при обработке запроса: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверяет, существует ли квартира с заданным ID.
        /// </summary>
        /// <param name="id">ID квартиры.</param>
        /// <returns>true если существует, иначе false</returns>
        private async Task<bool> ApartmentExists(int id)
        {
            return await _context.Apartment.AnyAsync(e => e.Id == id);
        }
    }
}