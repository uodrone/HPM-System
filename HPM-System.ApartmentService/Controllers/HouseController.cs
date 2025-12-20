using HPM_System.ApartmentService.DTOs.HousesDTOs;
using HPM_System.ApartmentService.Interfaces;
using HPM_System.ApartmentService.Models;
using HPM_System.ApartmentService.Services;
using Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.ApartmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HouseController : ControllerBase
    {
        private readonly IHouseRepository _houseRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly ILogger<HouseController> _logger;
        private readonly IUserServiceClient _userServiceClient;

        public HouseController(
            IHouseRepository houseRepository,
            IApartmentRepository apartmentRepository,
            ILogger<HouseController> logger,
            IUserServiceClient userServiceClient)
        {
            _houseRepository = houseRepository;
            _apartmentRepository = apartmentRepository;
            _logger = logger;
            _userServiceClient = userServiceClient;
        }

        /// <summary>
        /// Получить все дома
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HouseDto>>> GetHouses()
        {
            try
            {
                var houses = await _houseRepository.GetAllHousesAsync();
                return Ok(houses.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка домов");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить дом по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<HouseDto>> GetHouse(long id)
        {
            try
            {
                var house = await _houseRepository.GetHouseByIdAsync(id);

                if (house == null)
                {
                    return NotFound($"Дом с ID {id} не найден");
                }

                return Ok(MapToDto(house));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении дома с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить все дома, в которых у пользователя есть квартиры
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<HouseDto>>> GetHousesByUserId(Guid userId)
        {
            try
            {
                var houses = await _houseRepository.GetHousesByUserIdAsync(userId);
                return Ok(houses.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении домов для пользователя {UserId}", userId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новый дом
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<HouseDto>> CreateHouse(ManageHouseDto manageHouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var house = new House
                {
                    City = manageHouseDto.City,
                    Street = manageHouseDto.Street,
                    Number = manageHouseDto.Number,
                    Entrances = manageHouseDto.Entrances,
                    Floors = manageHouseDto.Floors,
                    HasGas = manageHouseDto.HasGas,
                    HasElectricity = manageHouseDto.HasElectricity,
                    HasElevator = manageHouseDto.HasElevator,
                    PostIndex = manageHouseDto.PostIndex,
                    ApartmentsArea = manageHouseDto.ApartmentsArea,
                    TotalArea = manageHouseDto.TotalArea,
                    LandArea = manageHouseDto.LandArea,
                    IsApartmentBuilding = manageHouseDto.IsApartmentBuilding,
                    HeadId = null, // по умолчанию никто не назначен
                    builtYear = manageHouseDto.builtYear
                };

                var createdHouse = await _houseRepository.CreateHouseAsync(house);
                await _houseRepository.SaveChangesAsync();

                return CreatedAtAction(nameof(GetHouse), new { id = createdHouse.Id }, MapToDto(createdHouse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании дома");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить дом
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHouse(long id, ManageHouseDto manageHouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var house = await _houseRepository.GetHouseByIdAsync(id);
                if (house == null)
                {
                    return NotFound($"Дом с ID {id} не найден");
                }

                house.City = manageHouseDto.City;
                house.Street = manageHouseDto.Street;
                house.Number = manageHouseDto.Number;
                house.Entrances = manageHouseDto.Entrances;
                house.Floors = manageHouseDto.Floors;
                house.HasGas = manageHouseDto.HasGas;
                house.HasElectricity = manageHouseDto.HasElectricity;
                house.HasElevator = manageHouseDto.HasElevator;
                house.PostIndex = manageHouseDto.PostIndex;
                house.ApartmentsArea = manageHouseDto.ApartmentsArea;
                house.TotalArea = manageHouseDto.TotalArea;
                house.LandArea = manageHouseDto.LandArea;
                house.IsApartmentBuilding = manageHouseDto.IsApartmentBuilding;
                house.builtYear = manageHouseDto.builtYear;

                var success = await _houseRepository.UpdateHouseAsync(house);
                if (!success)
                {
                    return NotFound($"Дом с ID {id} не найден");
                }

                await _houseRepository.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении дома с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить дом
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHouse(long id)
        {
            try
            {
                var success = await _houseRepository.DeleteHouseAsync(id);
                if (!success)
                {
                    return NotFound($"Дом с ID {id} не найден");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении дома с ID {Id}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Назначить старшего по дому
        /// </summary>
        [HttpPut("{houseId}/head/{userId}")]
        public async Task<IActionResult> AssignHead(long houseId, Guid userId)
        {
            try
            {
                var house = await _houseRepository.GetHouseByIdAsync(houseId);
                if (house == null)
                {
                    return NotFound($"Дом с ID {houseId} не найден");
                }

                var isUserLinked = await _houseRepository.IsUserLinkedToAnyApartmentInHouseAsync(houseId, userId);
                if (!isUserLinked)
                {
                    return BadRequest($"Пользователь с ID {userId} не связан ни с одной квартирой в доме {houseId}");
                }

                house.HeadId = userId;
                var success = await _houseRepository.UpdateHouseAsync(house);
                if (!success)
                {
                    return NotFound($"Дом с ID {houseId} не найден");
                }

                await _houseRepository.SaveChangesAsync();
                return Ok($"Пользователь {userId} назначен старшим по дому {houseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при назначении старшего по дому {HouseId}", houseId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Отозвать старшего по дому
        /// </summary>
        [HttpDelete("{houseId}/head")]
        public async Task<IActionResult> RevokeHead(long houseId)
        {
            try
            {
                var house = await _houseRepository.GetHouseByIdAsync(houseId);
                if (house == null)
                {
                    return NotFound($"Дом с ID {houseId} не найден");
                }

                if (house.HeadId == null)
                {
                    return BadRequest($"В доме {houseId} не назначен старший");
                }

                house.HeadId = null;
                var success = await _houseRepository.UpdateHouseAsync(house);
                if (!success)
                {
                    return NotFound($"Дом с ID {houseId} не найден");
                }

                await _houseRepository.SaveChangesAsync();
                return Ok($"Старший по дому {houseId} отозван");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отзыве старшего по дому {HouseId}", houseId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить информацию о старшем по дому (если назначен)
        /// </summary>
        [HttpGet("{houseId}/head")]
        public async Task<ActionResult<HouseHeadDto>> GetHead(long houseId)
        {
            try
            {
                var house = await _houseRepository.GetHouseByIdAsync(houseId);
                if (house == null)
                {
                    return NotFound($"Дом с ID {houseId} не найден");
                }

                if (house.HeadId == null)
                {
                    return NotFound($"В доме {houseId} не назначен старший");
                }

                // Получаем данные пользователя из UserService
                var headUser = await _userServiceClient.GetUserByIdAsync(house.HeadId.Value);
                if (headUser == null)
                {
                    return NotFound($"Пользователь-старший с ID {house.HeadId.Value} не найден в системе пользователей");
                }

                // Маппинг UserDto -> HouseHeadDto
                var headDto = new HouseHeadDto
                {
                    Id = headUser.Id,
                    FirstName = headUser.FirstName,
                    Patronymic = headUser.Patronymic,
                    PhoneNumber = headUser.PhoneNumber
                };

                return Ok(headDto);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка связи с UserService при получении данных старшего по дому {HouseId}", houseId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "В данный момент невозможно получить данные старшего. Сервис пользователей недоступен.", Details = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут при связи с UserService при получении данных старшего по дому {HouseId}", houseId);
                return StatusCode(StatusCodes.Status504GatewayTimeout,
                    new { Message = "Превышено время ожидания ответа от сервиса пользователей.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении старшего по дому {HouseId}", houseId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить информацию о старшем по дому по ID квартиры
        /// </summary>
        [HttpGet("apartment/{apartmentId}/head")]
        public async Task<ActionResult<HouseHeadDto>> GetHeadByApartmentId(long apartmentId)
        {
            try
            {
                // Получаем квартиру, чтобы узнать, к какому дому она относится
                var apartment = await _apartmentRepository.GetApartmentByIdAsync(apartmentId);
                if (apartment == null)
                    return NotFound($"Квартира с ID {apartmentId} не найдена");

                // Получаем дом по HouseId из квартиры
                var house = await _houseRepository.GetHouseByIdAsync(apartment.HouseId);
                if (house == null)
                    return NotFound($"Дом с ID {apartment.HouseId}, к которому относится квартира {apartmentId}, не найден");

                if (house.HeadId == null)
                    return NotFound($"В доме {house.Id} не назначен старший");

                // Получаем данные пользователя из UserService
                var headUser = await _userServiceClient.GetUserByIdAsync(house.HeadId.Value);
                if (headUser == null)
                    return NotFound($"Пользователь-старший с ID {house.HeadId.Value} не найден в системе пользователей");

                // Маппинг UserDto -> HouseHeadDto
                var headDto = new HouseHeadDto
                {
                    Id = headUser.Id,
                    FirstName = headUser.FirstName,
                    Patronymic = headUser.Patronymic,
                    PhoneNumber = headUser.PhoneNumber
                };

                return Ok(headDto);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка связи с UserService при получении данных старшего по квартире {ApartmentId}", apartmentId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "В данный момент невозможно получить данные старшего. Сервис пользователей недоступен.", Details = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут при связи с UserService при получении данных старшего по квартире {ApartmentId}", apartmentId);
                return StatusCode(StatusCodes.Status504GatewayTimeout,
                    new { Message = "Превышено время ожидания ответа от сервиса пользователей.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении старшего по квартире {ApartmentId}", apartmentId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить всех владельцев квартир в доме с номерами их квартир
        /// </summary>
        [HttpGet("{houseId}/owners")]
        public async Task<ActionResult<IEnumerable<HouseOwnerDto>>> GetHouseOwners(long houseId)
        {
            try
            {
                var house = await _houseRepository.GetHouseByIdAsync(houseId);
                if (house == null)
                {
                    return NotFound($"Дом с ID {houseId} не найден");
                }

                var apartments = await _apartmentRepository.GetApartmentsByHouseIdWithUsersAndStatusesAsync(houseId);

                // Группируем по UserId → список номеров квартир
                var ownerApartmentMap = new Dictionary<Guid, List<int>>();

                foreach (var apartment in apartments)
                {
                    foreach (var apartmentUser in apartment.Users)
                    {
                        if (apartmentUser.Statuses.Any(s => s.Status.Name == "Владелец"))
                        {
                            if (!ownerApartmentMap.ContainsKey(apartmentUser.UserId))
                            {
                                ownerApartmentMap[apartmentUser.UserId] = new List<int>();
                            }
                            ownerApartmentMap[apartmentUser.UserId].Add(apartment.Number);
                        }
                    }
                }

                var result = new List<HouseOwnerDto>();

                foreach (var (userId, apartmentNumbers) in ownerApartmentMap)
                {
                    try
                    {
                        var userDto = await _userServiceClient.GetUserByIdAsync(userId);
                        if (userDto != null)
                        {
                            result.Add(new HouseOwnerDto
                            {
                                UserId = userId,
                                FullName = $"{userDto.LastName} {userDto.FirstName} {userDto.Patronymic}".Trim(),
                                PhoneNumber = userDto.PhoneNumber,
                                ApartmentNumbers = apartmentNumbers.OrderBy(n => n).ToList() // сортируем для порядка
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Не удалось загрузить данные владельца с ID {UserId}", userId);
                        // Пропускаем проблемного пользователя, но не падаем
                    }
                }

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка связи с UserService при получении владельцев дома {HouseId}", houseId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { Message = "Сервис пользователей недоступен." });
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут при получении владельцев дома {HouseId}", houseId);
                return StatusCode(StatusCodes.Status504GatewayTimeout,
                    new { Message = "Превышено время ожидания ответа от сервиса пользователей." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении владельцев дома {HouseId}", houseId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        // Вспомогательный метод маппинга
        private HouseDto MapToDto(House house)
        {
            return new HouseDto
            {
                Id = house.Id,
                City = house.City,
                Street = house.Street,
                Number = house.Number,
                Entrances = house.Entrances,
                Floors = house.Floors,
                HasGas = house.HasGas,
                HasElectricity = house.HasElectricity,
                HasElevator = house.HasElevator,
                HeadId = house.HeadId,
                PostIndex = house.PostIndex,
                ApartmentsArea = house.ApartmentsArea,
                TotalArea = house.TotalArea,
                LandArea = house.LandArea,
                IsApartmentBuilding = house.IsApartmentBuilding,
                builtYear = house.builtYear
            };
        }
    }
}
