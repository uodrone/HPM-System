using DTOs.StatusDTOs;
using HPM_System.ApartmentService.Interfaces;
using HPM_System.ApartmentService.Models;
using HPM_System.ApartmentService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.ApartmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly IStatusRepository _statusRepository;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IStatusRepository statusRepository, ILogger<StatusController> logger)
        {
            _statusRepository = statusRepository;
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
                var statuses = await _statusRepository.GetAllStatusesAsync();
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
                var status = await _statusRepository.GetStatusByIdAsync(id);

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

                var existingStatus = await _statusRepository.FindStatusByNameAsync(createStatusDto.Name);

                if (existingStatus != null)
                {
                    return Conflict($"Статус с названием '{createStatusDto.Name}' уже существует");
                }

                var status = new Status
                {
                    Name = createStatusDto.Name.Trim()
                };

                var createdStatus = await _statusRepository.CreateStatusAsync(status);
                await _statusRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStatus), new { id = createdStatus.Id }, createdStatus);
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
                var status = await _statusRepository.GetStatusByIdAsync(id);

                if (status == null)
                {
                    return NotFound($"Статус с ID {id} не найден");
                }

                if (string.IsNullOrWhiteSpace(updateStatusDto.Name))
                {
                    return BadRequest("Название статуса не может быть пустым");
                }

                var existingStatus = await _statusRepository.FindStatusByNameAsync(updateStatusDto.Name, id);

                if (existingStatus != null)
                {
                    return Conflict($"Статус с названием '{updateStatusDto.Name}' уже существует");
                }

                status.Name = updateStatusDto.Name.Trim();
                var success = await _statusRepository.UpdateStatusAsync(status);
                if (!success)
                {
                    return NotFound($"Статус с ID {id} не найден");
                }

                await _statusRepository.SaveChangesAsync();
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
                var status = await _statusRepository.GetStatusByIdAsync(id);

                if (status == null)
                {
                    return NotFound($"Статус с ID {id} не найден");
                }

                var isUsed = await _statusRepository.IsStatusUsedAsync(id);
                if (isUsed)
                {
                    return BadRequest("Невозможно удалить статус, так как он используется пользователями");
                }

                var success = await _statusRepository.DeleteStatusAsync(id);
                if (!success)
                {
                    return NotFound($"Статус с ID {id} не найден");
                }

                await _statusRepository.SaveChangesAsync();
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
        public async Task<IActionResult> AssignStatusToUser(int apartmentId, Guid userId, int statusId)
        {
            try
            {
                var apartmentUser = await _statusRepository.GetApartmentUserAsync(apartmentId, userId);
                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                var status = await _statusRepository.GetStatusByIdAsync(statusId);
                if (status == null)
                {
                    return NotFound($"Статус с ID {statusId} не найден");
                }

                var existingStatus = await _statusRepository.GetApartmentUserStatusAsync(apartmentId, userId, statusId);
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

                await _statusRepository.AssignStatusToUserAsync(apartmentUserStatus);
                await _statusRepository.SaveChangesAsync();

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
        public async Task<IActionResult> RevokeStatusFromUser(int apartmentId, Guid userId, int statusId)
        {
            try
            {
                var apartmentUser = await _statusRepository.GetApartmentUserAsync(apartmentId, userId);
                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                var success = await _statusRepository.RevokeStatusFromUserAsync(apartmentId, userId, statusId);
                if (!success)
                {
                    return NotFound("Указанный статус не назначен пользователю для данной квартиры");
                }

                await _statusRepository.SaveChangesAsync();
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
        public async Task<ActionResult<IEnumerable<Status>>> GetUserStatusesForApartment(int apartmentId, Guid userId)
        {
            try
            {
                var apartmentUser = await _statusRepository.GetApartmentUserAsync(apartmentId, userId);
                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                var statuses = await _statusRepository.GetUserStatusesForApartmentAsync(apartmentId, userId);
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