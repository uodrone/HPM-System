using HPM_System.UserService.Data;
using HPM_System.UserService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HPM_System.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarsController> _logger;

        public CarsController(AppDbContext context, ILogger<CarsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Cars
        [HttpPost]
        public async Task<IActionResult> CreateCar([FromBody] Car car)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Проверяем, существует ли пользователь
                var userExists = await _context.Users.AnyAsync(u => u.Id == car.UserId);
                if (!userExists)
                {
                    return BadRequest(new { Message = $"Пользователь с ID {car.UserId} не найден" });
                }

                // Проверяем уникальность номера
                var existingCar = await _context.Cars.FirstOrDefaultAsync(c => c.Number == car.Number);
                if (existingCar != null)
                {
                    return BadRequest(new { Message = $"Автомобиль с номером {car.Number} уже существует" });
                }

                _context.Cars.Add(car);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCar), new { id = car.Id }, car);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка базы данных при создании автомобиля");

                // Более детальная обработка ошибок базы данных
                if (dbEx.InnerException?.Message.Contains("UNIQUE constraint failed") == true ||
                    dbEx.InnerException?.Message.Contains("duplicate key") == true)
                {
                    return BadRequest(new { Message = "Автомобиль с таким номером уже существует" });
                }

                if (dbEx.InnerException?.Message.Contains("FOREIGN KEY constraint failed") == true)
                {
                    return BadRequest(new { Message = "Указанный пользователь не существует" });
                }

                return StatusCode(500, new { Message = "Ошибка при сохранении в базу данных", Details = dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании автомобиля");
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", Details = ex.Message });
            }
        }

        // GET: api/Cars/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCar(long id)
        {
            try
            {
                var car = await _context.Cars.FindAsync(id);
                if (car == null) return NotFound();

                return Ok(car);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении автомобиля с ID {Id}", id);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // GET: api/Cars/by-user/{userId}
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetAllCarsByUserId(Guid userId)
        {
            try
            {
                var cars = await _context.Cars
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (!cars.Any())
                    return NotFound(new { Message = $"Автомобили пользователя с ID={userId} не найдены" });

                return Ok(cars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении автомобилей пользователя с ID={UserId}", userId);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // PUT: api/Cars/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar([FromBody] Car updatedCar)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                var existingCar = await _context.Cars.FindAsync(updatedCar.Id);

                if (existingCar == null)
                    return NotFound();

                existingCar.Mark = updatedCar.Mark;
                existingCar.Model = updatedCar.Model;
                existingCar.Color = updatedCar.Color;
                existingCar.Number = updatedCar.Number;
                existingCar.UserId = updatedCar.UserId;

                _context.Cars.Update(existingCar);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении автомобиля с ID {Id}", updatedCar.Id);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // DELETE: api/Cars/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(long id)
        {
            try
            {
                var car = await _context.Cars.FindAsync(id);

                if (car == null)
                    return NotFound();

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении автомобиля с ID {Id}", id);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }

        // DELETE: api/Cars/by-user/{userId}
        [HttpDelete("by-user/{userId}")]
        public async Task<IActionResult> DeleteAllCarsByUserId(Guid userId)
        {
            try
            {
                var cars = await _context.Cars
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (!cars.Any())
                    return NotFound(new { Message = $"Нет автомобилей для удаления у пользователя с ID={userId}" });

                _context.Cars.RemoveRange(cars);
                await _context.SaveChangesAsync();

                return Ok(new { Message = $"Успешно удалено {cars.Count} автомобилей у пользователя с ID={userId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении всех автомобилей пользователя с ID={UserId}", userId);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }
    }
}