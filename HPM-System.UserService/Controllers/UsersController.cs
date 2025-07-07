using HPM_System.UserService.Data;
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

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя");
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя с ID {Id}", id);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/Users/by-apartment/{apartmentId}
        [HttpGet("by-apartment/{apartmentId}")]
        public async Task<IActionResult> GetUsersByApartmentId(int apartmentId)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.ApartmentId.Contains(apartmentId))
                    .ToListAsync();

                if (!users.Any())
                    return NotFound(new { Message = $"Пользователи с ApartmentId={apartmentId} не найдены" });

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователей по id квартиры={ApartmentId}", apartmentId);
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
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            try
            {
                if (id != updatedUser.Id || !ModelState.IsValid)
                    return BadRequest();

                var existingUser = await _context.Users
                    .Include(u => u.Cars) // Сохраняем связь с автомобилями
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (existingUser == null)
                    return NotFound();

                // Обновляем поля
                existingUser.FirstName = updatedUser.FirstName;
                existingUser.LastName = updatedUser.LastName;
                existingUser.Patronymic = updatedUser.Patronymic;
                existingUser.Email = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.Age = updatedUser.Age;
                existingUser.ApartmentId = updatedUser.ApartmentId;
                existingUser.EventId = updatedUser.EventId;
                existingUser.NotificationtId = updatedUser.NotificationtId;
                existingUser.VotingId = updatedUser.VotingId;
                existingUser.CommunityId = updatedUser.CommunityId;
                existingUser.Cars = updatedUser.Cars;

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
        public async Task<IActionResult> DeleteUser(int id)
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
    }
}