using HPM_System.ApartmentService.Data;
using HPM_System.ApartmentService.Models;
using HPM_System.ApartmentService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.ApartmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatusController> _logger;

        public StatusController(AppDbContext context, ILogger<StatusController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить все доступные статусы
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Status>>> GetStatuses()
        {
            try
            {
                var statuses = await _context.Statuses.ToListAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка статусов");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить статус по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Status>> GetStatus(int id)
        {
            try
            {
                var status = await _context.Statuses.FindAsync(id);

                if (status == null)
                {
                    return NotFound($"Статус с ID {id} не найден");
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статуса с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новый статус
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Status>> CreateStatus(CreateStatusDto createStatusDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createStatusDto.Name))
                {
                    return BadRequest("Название статуса не может быть пустым");
                }

                // Проверяем уникальность названия
                var existingStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == createStatusDto.Name.ToLower());

                if (existingStatus != null)
                {
                    return Conflict($"Статус с названием '{createStatusDto.Name}' уже существует");
                }

                var status = new Status
                {
                    Name = createStatusDto.Name.Trim()
                };

                _context.Statuses.Add(status);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStatus), new { id = status.Id }, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании статуса");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить статус
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto updateStatusDto)
        {
            try
            {
                var status = await _context.Statuses.FindAsync(id);

                if (status == null)
                {
                    return NotFound($"Статус с ID {id} не найден");
                }

                if (string.IsNullOrWhiteSpace(updateStatusDto.Name))
                {
                    return BadRequest("Название статуса не может быть пустым");
                }

                // Проверяем уникальность названия (исключая текущий статус)
                var existingStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == updateStatusDto.Name.ToLower() && s.Id != id);

                if (existingStatus != null)
                {
                    return Conflict($"Статус с названием '{updateStatusDto.Name}' уже существует");
                }

                status.Name = updateStatusDto.Name.Trim();
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить статус
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            try
            {
                var status = await _context.Statuses.FindAsync(id);

                if (status == null)
                {
                    return NotFound($"Статус с ID {id} не найден");
                }

                // Проверяем, используется ли статус
                var isUsed = await _context.ApartmentUserStatuses
                    .AnyAsync(aus => aus.StatusId == id);

                if (isUsed)
                {
                    return BadRequest("Невозможно удалить статус, так как он используется пользователями");
                }

                _context.Statuses.Remove(status);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении статуса с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Назначить статус пользователю для квартиры
        /// </summary>
        [HttpPost("apartment/{apartmentId}/user/{userId}/status/{statusId}")]
        public async Task<IActionResult> AssignStatusToUser(int apartmentId, int userId, int statusId)
        {
            try
            {
                // Проверяем существование квартиры, пользователя и статуса
                var apartmentUser = await _context.ApartmentUsers
                    .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);

                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                var status = await _context.Statuses.FindAsync(statusId);
                if (status == null)
                {
                    return NotFound($"Статус с ID {statusId} не найден");
                }

                // Проверяем, не назначен ли уже такой статус
                var existingStatus = await _context.ApartmentUserStatuses
                    .FirstOrDefaultAsync(aus => aus.ApartmentId == apartmentId &&
                                              aus.UserId == userId &&
                                              aus.StatusId == statusId);

                if (existingStatus != null)
                {
                    return Conflict("Данный статус уже назначен пользователю для этой квартиры");
                }

                var apartmentUserStatus = new ApartmentUserStatus
                {
                    ApartmentId = apartmentId,
                    UserId = userId,
                    StatusId = statusId
                };

                _context.ApartmentUserStatuses.Add(apartmentUserStatus);
                await _context.SaveChangesAsync();

                return Ok("Статус успешно назначен пользователю");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при назначении статуса пользователю");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Отозвать статус у пользователя для квартиры
        /// </summary>
        [HttpDelete("apartment/{apartmentId}/user/{userId}/status/{statusId}")]
        public async Task<IActionResult> RevokeStatusFromUser(int apartmentId, int userId, int statusId)
        {
            try
            {
                var apartmentUser = await _context.ApartmentUsers
                    .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);

                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                var apartmentUserStatus = await _context.ApartmentUserStatuses
                    .FirstOrDefaultAsync(aus => aus.ApartmentId == apartmentId &&
                                              aus.UserId == userId &&
                                              aus.StatusId == statusId);

                if (apartmentUserStatus == null)
                {
                    return NotFound("Указанный статус не назначен пользователю для данной квартиры");
                }

                _context.ApartmentUserStatuses.Remove(apartmentUserStatus);
                await _context.SaveChangesAsync();

                return Ok("Статус успешно отозван у пользователя");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отзыве статуса у пользователя");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить все статусы пользователя для квартиры
        /// </summary>
        [HttpGet("apartment/{apartmentId}/user/{userId}")]
        public async Task<ActionResult<IEnumerable<Status>>> GetUserStatusesForApartment(int apartmentId, int userId)
        {
            try
            {
                var apartmentUser = await _context.ApartmentUsers
                    .FirstOrDefaultAsync(au => au.ApartmentId == apartmentId && au.UserId == userId);

                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                var statuses = await _context.ApartmentUserStatuses
                    .Where(aus => aus.ApartmentId == apartmentId && aus.UserId == userId)
                    .Include(aus => aus.Status)
                    .Select(aus => aus.Status)
                    .ToListAsync();

                return Ok(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статусов пользователя");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}