using DTOs.ApartmentDTOs;
using DTOs.ShareDTOs;
using DTOs.StatusDTOs;
using DTOs.UserDTOs;
using HPM_System.ApartmentService.Interfaces;
using HPM_System.ApartmentService.Models;
using HPM_System.ApartmentService.Repositories;
using HPM_System.ApartmentService.Services;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HPM_System.ApartmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApartmentController : ControllerBase
    {
        private readonly IApartmentRepository _apartmentRepository;
        private readonly ILogger<ApartmentController> _logger;
        private readonly IUserServiceClient _userServiceClient;

        public ApartmentController(
            IApartmentRepository apartmentRepository,
            ILogger<ApartmentController> logger,
            IUserServiceClient userServiceClient)
        {
            _apartmentRepository = apartmentRepository;
            _logger = logger;
            _userServiceClient = userServiceClient;
        }

        /// <summary>
        /// Получить все квартиры (краткая информация)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApartmentListResponseDto>>> GetApartments()
        {
            try
            {
                var apartments = await _apartmentRepository.GetAllApartmentsAsync();

                var result = apartments.Select(a => new ApartmentListResponseDto
                {
                    Id = a.Id,
                    Number = a.Number,
                    NumbersOfRooms = a.NumbersOfRooms,
                    ResidentialArea = a.ResidentialArea,
                    TotalArea = a.TotalArea,
                    Floor = a.Floor,
                    HouseId = a.HouseId,
                    UsersCount = a.Users.Count,
                    OwnersCount = a.Users.Count(au => au.Statuses.Any(s => s.Status.Name == "Владелец"))
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка квартир");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить квартиру по ID (с полной информацией о пользователях)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApartmentResponseDto>> GetApartment(long id)
        {
            try
            {
                var apartment = await _apartmentRepository.GetApartmentByIdAsync(id);

                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {id} не найдена");
                }

                var result = await MapToApartmentResponseDto(apartment);
                return Ok(result);
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
        public async Task<ActionResult<IEnumerable<ApartmentResponseDto>>> GetApartmentsByUserId(Guid userId)
        {
            try
            {
                var userExists = await _userServiceClient.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound($"Пользователь с ID {userId} не найден");
                }

                var apartments = await _apartmentRepository.GetApartmentsByUserIdAsync(userId);

                var result = new List<ApartmentResponseDto>();
                foreach (var apartment in apartments)
                {
                    var dto = await MapToApartmentResponseDto(apartment);
                    result.Add(dto);
                }

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка связи с UserService при проверке пользователя {UserId}", userId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "В данный момент невозможно проверить пользователя. Сервис пользователей недоступен.", Details = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
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
        public async Task<ActionResult<IEnumerable<ApartmentResponseDto>>> GetApartmentsByUserPhone(string phone)
        {
            try
            {
                var user = await _userServiceClient.GetUserByPhoneAsync(phone);
                if (user == null)
                {
                    return NotFound($"Пользователь с телефоном {phone} не найден");
                }

                var apartments = await _apartmentRepository.GetApartmentsByUserIdAsync(user.Id);

                var result = new List<ApartmentResponseDto>();
                foreach (var apartment in apartments)
                {
                    var dto = await MapToApartmentResponseDto(apartment);
                    result.Add(dto);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении квартир для пользователя с телефоном {Phone}", phone);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить квартиры по ID дома
        /// </summary>
        [HttpGet("house/{houseid}")]
        public async Task<ActionResult<IEnumerable<ApartmentResponseDto>>> GetApartmentsByHouseId(long houseId)
        {
            try
            {
                var apartments = await _apartmentRepository.GetApartmentsByHouseIdAsync(houseId);

                var result = new List<ApartmentResponseDto>();
                foreach (var apartment in apartments)
                {
                    var dto = await MapToApartmentResponseDto(apartment);
                    result.Add(dto);
                }

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка связи с сервисом пользователей");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "В данный момент невозможно проверить пользователя. Сервис пользователей недоступен.", Details = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут при связи с UserService");
                return StatusCode(StatusCodes.Status504GatewayTimeout,
                    new { Message = "Превышено время ожидания ответа от сервиса пользователей.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новую квартиру
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApartmentResponseDto>> CreateApartment(Apartment apartment)
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

                if (apartment.Users == null)
                {
                    apartment.Users = new List<ApartmentUser>();
                }

                var createdApartment = await _apartmentRepository.CreateApartmentAsync(apartment);

                // Перезагружаем с включенными связями (если нужно — можно вынести в репозиторий метод ReloadWithIncludes)
                var apartmentWithUsers = await _apartmentRepository.GetApartmentByIdAsync(createdApartment.Id);

                var result = await MapToApartmentResponseDto(apartmentWithUsers!);
                return CreatedAtAction(nameof(GetApartment), new { id = apartment.Id }, result);
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
        public async Task<IActionResult> UpdateApartment(long id, Apartment apartment)
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

                var success = await _apartmentRepository.UpdateApartmentAsync(apartment);
                if (!success)
                {
                    return NotFound($"Квартира с ID {id} не найдена");
                }

                return NoContent();
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
        public async Task<IActionResult> DeleteApartment(long id)
        {
            try
            {
                var success = await _apartmentRepository.DeleteApartmentAsync(id);
                if (!success)
                {
                    return NotFound($"Квартира с ID {id} не найдена");
                }

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
        public async Task<IActionResult> AddUserToApartment(long apartmentId, Guid userId)
        {
            try
            {
                var apartment = await _apartmentRepository.GetApartmentByIdAsync(apartmentId);
                if (apartment == null)
                {
                    return NotFound($"Квартира с ID {apartmentId} не найдена");
                }

                var userExists = await _userServiceClient.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound($"Пользователь с ID {userId} не найден");
                }

                if (apartment.Users.Any(u => u.UserId == userId))
                {
                    return Conflict("Пользователь уже привязан к этой квартире");
                }

                await _apartmentRepository.AddUserToApartmentAsync(apartmentId, userId);
                await _apartmentRepository.SaveChangesAsync();

                return Ok("Пользователь успешно добавлен к квартире");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка связи с UserService при добавлении пользователя {UserId} к квартире {ApartmentId}", userId, apartmentId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "В данный момент невозможно проверить пользователя. Сервис пользователей недоступен.", Details = ex.Message });
            }
            catch (TaskCanceledException ex)
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
        public async Task<IActionResult> RemoveUserFromApartment(long apartmentId, Guid userId)
        {
            try
            {
                var success = await _apartmentRepository.RemoveUserFromApartmentAsync(apartmentId, userId);
                if (!success)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

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
        public async Task<IActionResult> UpdateUserShare(long apartmentId, Guid userId, [FromBody] UpdateShareDto updateShareDto)
        {
            try
            {
                if (updateShareDto.Share < 0 || updateShareDto.Share > 1)
                {
                    return BadRequest("Доля владения должна быть от 0 до 1");
                }

                var apartmentUser = await _apartmentRepository.GetUserApartmentLinkAsync(apartmentId, userId);
                if (apartmentUser == null)
                {
                    return NotFound("Пользователь не связан с указанной квартирой");
                }

                var isOwner = apartmentUser.Statuses.Any(aus => aus.Status.Name == "Владелец");
                if (!isOwner)
                {
                    return BadRequest("Долю владения можно устанавливать только для владельцев");
                }

                apartmentUser.Share = updateShareDto.Share;
                await _apartmentRepository.SaveChangesAsync();

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
        public async Task<ActionResult<IEnumerable<UserShareDto>>> GetApartmentShares(long apartmentId)
        {
            try
            {
                var apartment = await _apartmentRepository.GetApartmentByIdAsync(apartmentId);
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
        public async Task<ActionResult<ApartmentStatisticsDto>> GetApartmentStatistics(long apartmentId)
        {
            try
            {
                var apartment = await _apartmentRepository.GetApartmentByIdAsync(apartmentId);
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

        // Вспомогательный метод для маппинга в DTO
        private async Task<ApartmentResponseDto> MapToApartmentResponseDto(Apartment apartment)
        {
            var result = new ApartmentResponseDto
            {
                Id = apartment.Id,
                Number = apartment.Number,
                NumbersOfRooms = apartment.NumbersOfRooms,
                ResidentialArea = apartment.ResidentialArea,
                TotalArea = apartment.TotalArea,
                Floor = apartment.Floor,
                HouseId = apartment.HouseId
            };

            foreach (var apartmentUser in apartment.Users)
            {
                UserDto? userDetails = null;
                try
                {
                    userDetails = await _userServiceClient.GetUserByIdAsync(apartmentUser.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось получить данные пользователя {UserId}", apartmentUser.UserId);
                }

                var userDto = new ApartmentUserResponseDto
                {
                    UserId = apartmentUser.UserId,
                    Share = apartmentUser.Share,
                    UserDetails = userDetails,
                    Statuses = apartmentUser.Statuses.Select(s => new StatusDto
                    {
                        Id = s.Status.Id,
                        Name = s.Status.Name
                    }).ToList()
                };

                result.Users.Add(userDto);
            }

            return result;
        }
    }
}