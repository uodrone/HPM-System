﻿using HPM_System.UserService.Data;
using HPM_System.UserService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HPM_System.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Проверка на уникальность Email перед добавлением
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());

                if (existingUser != null)
                {
                    // Возвращаем 409 Conflict с понятным сообщением
                    return Conflict(new { Message = $"Пользователь с email '{user.Email}' уже существует." });
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            // Явная обработка ошибки уникальности из БД (на случай, если проверка выше как-то "проскочила")
            catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("duplicate key value violates unique constraint \"IX_Users_Email\"") == true)
            {
                _logger.LogWarning(dbEx, "Попытка создания пользователя с уже существующим email: {Email}", user.Email);
                // Возвращаем 409 Conflict с понятным сообщением
                return Conflict(new { Message = $"Пользователь с email '{user.Email}' уже существует." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя");
                // Возвращаем 500 Internal Server Error с общим сообщением
                // (детали логируются на сервере)
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера при создании пользователя." });
            }
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                Guid userId = Guid.Parse(id);
                //грузим пользака сразу с его автомобилями
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return NotFound();

                return Ok(user);
            }
            catch (FormatException fm)
            {
                _logger.LogError(fm, "Неверный формат ID пользователя {Id}", id);
                return StatusCode(500, new { Message = "Неверный формат ID пользователя" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя с ID {Id}", id);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: /api/users/by-ids?ids=1,2,3
        [HttpGet("by-ids")]
        public async Task<IActionResult> GetUsersByIds([FromQuery] Guid[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    _logger.LogWarning("Не переданы ID пользователей");
                    return BadRequest(new { Message = "Не переданы ID пользователей" });
                }

                var users = await _context.Users
                    .Where(u => ids.Contains(u.Id))
                    .ToListAsync();

                if (!users.Any())
                {
                    _logger.LogInformation("Пользователи с указанными ID не найдены: {@Ids}", ids);
                    return NotFound(new { Message = "Пользователи не найдены" });
                }

                _logger.LogInformation("Найдено {Count} пользователей по списку ID", users.Count);

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователей по ID");
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/Users/by-phone/{phone}
        [HttpGet("by-phone/{phone}")]
        public async Task<IActionResult> GetUserByPhone(string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                {
                    _logger.LogWarning("Получен пустой номер телефона");
                    return BadRequest(new { Message = "Номер телефона не может быть пустым" });
                }

                // Декодируем URL-кодированный номер телефона
                string decodedPhone = Uri.UnescapeDataString(phone);
                _logger.LogDebug("Поиск пользователя по телефону. Исходный: {OriginalPhone}, декодированный: {DecodedPhone}", phone, decodedPhone);

                // Нормализуем номер телефона для поиска (убираем пробелы, дефисы и скобки)
                string normalizedPhone = NormalizePhoneNumber(decodedPhone);
                string normalizedSearchPhone = NormalizePhoneNumber(phone);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.PhoneNumber == decodedPhone ||
                        u.PhoneNumber == phone ||
                        u.PhoneNumber == normalizedPhone ||
                        u.PhoneNumber == normalizedSearchPhone);

                if (user == null)
                {
                    _logger.LogInformation("Пользователь с номером телефона {Phone} не найден", decodedPhone);
                    return NotFound(new { Message = $"Пользователь с номером телефона {decodedPhone} не найден" });
                }

                _logger.LogInformation("Найден пользователь {UserId} по номеру телефона {Phone}", user.Id, decodedPhone);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по номеру телефона {Phone}", phone);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/Users/by-car-number/{carNumber}
        [HttpGet("by-car-number/{carNumber}")]
        public async Task<IActionResult> GetUserByCarNumber(string carNumber)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Cars)
                    .FirstOrDefaultAsync(u => u.Cars.Any(c => c.Number == carNumber));

                if (user == null)
                    return NotFound(new { Message = $"Пользователь с автомобилем номер {carNumber} не найден" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по номеру автомобиля={CarNumber}", carNumber);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей");
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, User updatedUser)
        {
            try
            {
                if (id != updatedUser.Id || !ModelState.IsValid)
                    return BadRequest();

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (existingUser == null)
                    return NotFound();

                // Обновляем поля
                existingUser.FirstName = updatedUser.FirstName;
                existingUser.LastName = updatedUser.LastName;
                existingUser.Patronymic = updatedUser.Patronymic;
                existingUser.Email = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.Birthday = updatedUser.Birthday;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                return NoContent(); // 204 - успешно обновлено
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пользователя с ID {Id}", updatedUser.Id);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _context.Users
                .Include(u => u.Cars)
                .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return NotFound();

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent(); // 204 - успешно удалено
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя с ID {id}", id);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Нормализует номер телефона для поиска
        /// </summary>
        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            // Убираем все символы кроме цифр и знака +
            return System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d+]", "");
        }
    }
}