using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HPM_System.ApartmentService.Data;
using HPM_System.ApartmentService.Models;

namespace HPM_System.ApartmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApartmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApartmentController> _logger;

        public ApartmentController(AppDbContext context, ILogger<ApartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить все квартиры
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartments()
        {
            try
            {
                var apartments = await _context.Apartment.ToListAsync();
                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка квартир");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить квартиру по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Apartment>> GetApartment(int id)
        {
            try
            {
                var apartment = await _context.Apartment.FindAsync(id);

                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {id} не найдена");
                }

                return Ok(apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении квартиры с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить квартиры по ID пользователя
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartmentsByUserId(int userId)
        {
            try
            {
                var apartments = await _context.Apartment
                    .Where(a => a.UserId != null && a.UserId.Contains(userId))
                    .ToListAsync();

                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении квартир для пользователя {UserId}", userId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новую квартиру
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Apartment>> CreateApartment(Apartment apartment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Валидация данных
                if (apartment.Number <= 0)
                {
                    return BadRequest("Номер квартиры должен быть положительным числом");
                }

                if (apartment.NumbersOfRooms <= 0)
                {
                    return BadRequest("Количество комнат должно быть положительным числом");
                }

                if (apartment.ResidentialArea <= 0 || apartment.TotalArea <= 0)
                {
                    return BadRequest("Площадь должна быть положительным числом");
                }

                if (apartment.ResidentialArea > apartment.TotalArea)
                {
                    return BadRequest("Жилая площадь не может быть больше общей площади");
                }

                _context.Apartment.Add(apartment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetApartment), new { id = apartment.Id }, apartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании квартиры");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить квартиру
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApartment(int id, Apartment apartment)
        {
            try
            {
                if (id != apartment.Id)
                {
                    return BadRequest("ID в URL не совпадает с ID в теле запроса");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Валидация данных
                if (apartment.Number <= 0)
                {
                    return BadRequest("Номер квартиры должен быть положительным числом");
                }

                if (apartment.NumbersOfRooms <= 0)
                {
                    return BadRequest("Количество комнат должно быть положительным числом");
                }

                if (apartment.ResidentialArea <= 0 || apartment.TotalArea <= 0)
                {
                    return BadRequest("Площадь должна быть положительным числом");
                }

                if (apartment.ResidentialArea > apartment.TotalArea)
                {
                    return BadRequest("Жилая площадь не может быть больше общей площади");
                }





                _context.Entry(apartment).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApartmentExists(id))
                {
                    return NotFound($"Квартира с ID {id} не найдена");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении квартиры с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить квартиру
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApartment(int id)
        {
            try
            {
                var apartment = await _context.Apartment.FindAsync(id);
                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {id} не найдена");
                }

                _context.Apartment.Remove(apartment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении квартиры с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Добавить пользователя к квартире
        /// </summary>
        [HttpPost("{apartmentId}/users/{userId}")]
        public async Task<IActionResult> AddUserToApartment(int apartmentId, int userId)
        {
            try
            {
                var apartment = await _context.Apartment.FindAsync(apartmentId);
                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {apartmentId} не найдена");
                }

                if (apartment.UserId == null)
                {
                    apartment.UserId = new List<int>();
                }

                if (apartment.UserId.Contains(userId))
                {
                    return Conflict($"Пользователь {userId} уже привязан к квартире {apartmentId}");
                }

                apartment.UserId.Add(userId);
                await _context.SaveChangesAsync();

                return Ok($"Пользователь {userId} успешно добавлен к квартире {apartmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении пользователя {UserId} к квартире {ApartmentId}", userId, apartmentId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить пользователя из квартиры
        /// </summary>
        [HttpDelete("{apartmentId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromApartment(int apartmentId, int userId)
        {
            try
            {
                var apartment = await _context.Apartment.FindAsync(apartmentId);
                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {apartmentId} не найдена");
                }

                if (apartment.UserId == null || !apartment.UserId.Contains(userId))
                {
                    return NotFound($"Пользователь {userId} не найден в квартире {apartmentId}");
                }

                if (apartment.UserId.Count == 1)
                {
                    // Просто очищаем список пользователей вместо удаления квартиры
                    apartment.UserId.Clear();
                }
                else
                {
                    apartment.UserId.Remove(userId);
                }
                await _context.SaveChangesAsync();

                return Ok($"Пользователь {userId} успешно удален из квартиры {apartmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя {UserId} из квартиры {ApartmentId}", userId, apartmentId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Проверить существование квартиры
        /// </summary>
        private bool ApartmentExists(int id)
        {
            return _context.Apartment.Any(e => e.Id == id);
        }
    }
}