using HPM_System.ApartmentService.Data;
using HPM_System.ApartmentService.Models;
using HPM_System.ApartmentService.Services;
using HPM_System.ApartmentService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using HPM_System.ApartmentService.DTOs;

namespace HPM_System.ApartmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApartmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ApartmentController> _logger;
        private readonly IUserServiceClient _userServiceClient;

        public ApartmentController(AppDbContext context, ILogger<ApartmentController> logger, IUserServiceClient userServiceClient)
        {
            _context = context;
            _logger = logger;
            _userServiceClient = userServiceClient;
        }

        /// <summary>
        /// Получить все квартиры (с пользователями и их статусами)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartments()
        {
            try
            {
                var apartments = await _context.Apartment
                    .Include(a => a.Users)
                        .ThenInclude(au => au.User)
                    .Include(a => a.Users)
                        .ThenInclude(au => au.Statuses)
                            .ThenInclude(aus => aus.Status)
                    .ToListAsync();

                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка квартир");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить квартиру по ID (с пользователями и их статусами)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Apartment>> GetApartment(int id)
        {
            try
            {
                var apartment = await _context.Apartment
                    .Include(a => a.Users)
                        .ThenInclude(au => au.User)
                    .Include(a => a.Users)
                        .ThenInclude(au => au.Statuses)
                            .ThenInclude(aus => aus.Status)
                    .FirstOrDefaultAsync(a => a.Id == id);

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
                // Проверяем существование пользователя в UserService
                var userExists = await _userServiceClient.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound($"Пользователь с ID {userId} не найден");
                }

                var apartments = await _context.Apartment
                    .Where(a => a.Users.Any(u => u.UserId == userId))
                    .Include(a => a.Users)
                        .ThenInclude(au => au.User)
                    .Include(a => a.Users)
                        .ThenInclude(au => au.Statuses)
                            .ThenInclude(aus => aus.Status)
                    .ToListAsync();

                return Ok(apartments);
            }
            catch (HttpRequestException ex)
            {
                // Это означает, что UserService недоступен (DNS, отказ в подключении, 502/503 от прокси и т.д.)
                _logger.LogError(ex, "Ошибка связи с UserService при проверке пользователя {UserId}", userId);
                // Можно вернуть более конкретный статус, например, 503 если точно знаем, что UserService недоступен
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "В данный момент невозможно проверить пользователя. Сервис пользователей недоступен.", Details = ex.Message });
            }
            catch (TaskCanceledException ex) // Таймаут
            {
                // Это означает, что UserService не ответил вовремя
                _logger.LogError(ex, "Таймаут при связи с UserService при проверке пользователя {UserId}", userId);
                return StatusCode(StatusCodes.Status504GatewayTimeout,
                    new { Message = "Превышено время ожидания ответа от сервиса пользователей.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении квартир для пользователя {UserId}", userId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить квартиры по номеру телефона пользователя
        /// </summary>
        [HttpGet("phone/{phone}")]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartmentsByUserPhone(string phone)
        {
            try
            {
                // Получаем пользователя из UserService по номеру телефона
                var user = await _userServiceClient.GetUserByPhoneAsync(phone);
                if (user == null)
                {
                    return NotFound($"Пользователь с телефоном {phone} не найден");
                }

                var apartments = await _context.Apartment
                    .Where(a => a.Users.Any(u => u.UserId == user.Id))
                    .Include(a => a.Users)
                        .ThenInclude(au => au.User)
                    .Include(a => a.Users)
                        .ThenInclude(au => au.Statuses)
                            .ThenInclude(aus => aus.Status)
                    .ToListAsync();

                return Ok(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении квартир для пользователя с телефоном {Phone}", phone);
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
                if (apartment.HouseId <= 0)
                {
                    return BadRequest("ID дома должен быть положительным числом");
                }

                // Инициализируем связь, если она была передана как null
                if (apartment.Users == null)
                {
                    apartment.Users = new List<ApartmentUser>();
                }

                _context.Apartment.Add(apartment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetApartment), new { id = apartment.Id }, apartment);
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder("Ошибка при создании квартиры");
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                errorMessage.AppendLine(": " + inner.Message);
                _logger.LogError(ex, errorMessage.ToString());
                return StatusCode(500, errorMessage.ToString());
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
                if (apartment.HouseId <= 0)
                {
                    return BadRequest("ID дома должен быть положительным числом");
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
                var apartment = await _context.Apartment
                    .Include(a => a.Users)
                        .ThenInclude(au => au.Statuses)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {id} не найдена");
                }

                // Удаляем все связанные статусы пользователей
                var allStatuses = apartment.Users.SelectMany(au => au.Statuses).ToList();
                _context.ApartmentUserStatuses.RemoveRange(allStatuses);

                // Удаляем квартиру (связи с пользователями удалятся каскадно)
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
        public async Task<IActionResult> AddUserToApartment(int apartmentId, int userId, [FromBody] AddUserToApartmentDto? dto = null)
        {
            try
            {
                var apartment = await _context.Apartment
                    .Include(a => a.Users)
                    .FirstOrDefaultAsync(a => a.Id == apartmentId);

                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {apartmentId} не найдена");
                }

                // Проверяем существование пользователя в UserService
                var userExists = await _userServiceClient.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound($"Пользователь с ID {userId} не найден");
                }

                // Проверяем, не привязан ли уже пользователь к квартире
                if (apartment.Users.Any(u => u.UserId == userId))
                {
                    return Conflict("Пользователь уже привязан к этой квартире");
                }

                // Создаем или находим пользователя в локальной БД
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    user = new User { Id = userId };
                    _context.Users.Add(user);
                }

                var apartmentUser = new ApartmentUser
                {
                    ApartmentId = apartmentId,
                    UserId = userId,
                    Share = dto?.Share ?? 0m
                };

                apartment.Users.Add(apartmentUser);
                await _context.SaveChangesAsync();

                return Ok("Пользователь успешно добавлен к квартире");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка связи с UserService при добавлении пользователя {UserId} к квартире {ApartmentId}", userId, apartmentId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "В данный момент невозможно проверить пользователя. Сервис пользователей недоступен.", Details = ex.Message });
            }
            catch (TaskCanceledException ex) // Таймаут
            {
                _logger.LogError(ex, "Таймаут при связи с UserService при добавлении пользователя {UserId} к квартире {ApartmentId}", userId, apartmentId);
                return StatusCode(StatusCodes.Status504GatewayTimeout,
                    new { Message = "Превышено время ожидания ответа от сервиса пользователей.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении пользователя к квартире");
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
                var apartmentUser = await _context.ApartmentUsers
                    .Include(au => au.Statuses)
                    .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);

                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                // Удаляем все статусы пользователя для этой квартиры
                _context.ApartmentUserStatuses.RemoveRange(apartmentUser.Statuses);

                // Удаляем связь пользователя с квартирой
                _context.ApartmentUsers.Remove(apartmentUser);
                await _context.SaveChangesAsync();

                return Ok("Пользователь успешно удален из квартиры");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя из квартиры");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Установить или обновить долю владения пользователя (только для владельцев)
        /// </summary>
        [HttpPut("{apartmentId}/users/{userId}/share")]
        public async Task<IActionResult> UpdateUserShare(int apartmentId, int userId, [FromBody] UpdateShareDto updateShareDto)
        {
            try
            {
                if (updateShareDto.Share < 0 || updateShareDto.Share > 1)
                {
                    return BadRequest("Доля владения должна быть от 0 до 1");
                }

                var apartmentUser = await _context.ApartmentUsers
                    .Include(au => au.Statuses)
                        .ThenInclude(aus => aus.Status)
                    .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);

                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                // Проверяем, является ли пользователь владельцем
                var isOwner = apartmentUser.Statuses.Any(aus => aus.Status.Name == "Владелец");
                if (!isOwner)
                {
                    return BadRequest("Долю владения можно устанавливать только для владельцев");
                }

                apartmentUser.Share = updateShareDto.Share;
                await _context.SaveChangesAsync();

                return Ok($"Доля владения пользователя обновлена до {updateShareDto.Share:P}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении доли владения пользователя");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить информацию о долях владения для квартиры
        /// </summary>
        [HttpGet("{apartmentId}/shares")]
        public async Task<ActionResult<IEnumerable<UserShareDto>>> GetApartmentShares(int apartmentId)
        {
            try
            {
                var apartment = await _context.Apartment
                    .Include(a => a.Users)
                        .ThenInclude(au => au.Statuses)
                            .ThenInclude(aus => aus.Status)
                    .FirstOrDefaultAsync(a => a.Id == apartmentId);

                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {apartmentId} не найдена");
                }

                var shares = apartment.Users
                    .Where(au => au.Statuses.Any(aus => aus.Status.Name == "Владелец"))
                    .Select(au => new UserShareDto
                    {
                        UserId = au.UserId,
                        Share = au.Share
                    })
                    .ToList();

                return Ok(shares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении долей владения для квартиры {ApartmentId}", apartmentId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить статистику по квартире
        /// </summary>
        [HttpGet("{apartmentId}/statistics")]
        public async Task<ActionResult<ApartmentStatisticsDto>> GetApartmentStatistics(int apartmentId)
        {
            try
            {
                var apartment = await _context.Apartment
                    .Include(a => a.Users)
                        .ThenInclude(au => au.Statuses)
                            .ThenInclude(aus => aus.Status)
                    .FirstOrDefaultAsync(a => a.Id == apartmentId);

                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {apartmentId} не найдена");
                }

                var statistics = new ApartmentStatisticsDto
                {
                    ApartmentId = apartmentId,
                    TotalUsers = apartment.Users.Count,
                    OwnersCount = apartment.Users.Count(au => au.Statuses.Any(s => s.Status.Name == "Владелец")),
                    TenantsCount = apartment.Users.Count(au => au.Statuses.Any(s => s.Status.Name == "Жилец")),
                    RegisteredCount = apartment.Users.Count(au => au.Statuses.Any(s => s.Status.Name == "Прописан")),
                    TotalOwnershipShare = apartment.Users
                        .Where(au => au.Statuses.Any(s => s.Status.Name == "Владелец"))
                        .Sum(au => au.Share)
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики для квартиры {ApartmentId}", apartmentId);
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