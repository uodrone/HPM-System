using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace HPM_System.IdentityServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Неверная модель запроса");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Попытка регистрации: {Email}", model.Email);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errorDetails = new List<object>();

                foreach (var error in result.Errors)
                {
                    _logger.LogError("Ошибка регистрации: {Code} — {Description}",
                        error.Code, error.Description);

                    // Детализация ошибок в зависимости от кода
                    var errorDetail = GetDetailedErrorMessage(error);
                    errorDetails.Add(new
                    {
                        Code = error.Code,
                        Description = error.Description,
                        DetailedMessage = errorDetail
                    });
                }

                return BadRequest(new
                {
                    Message = "Ошибка регистрации пользователя",
                    Errors = errorDetails
                });
            }

            // Создаем пользователя в UserService
            var userServiceCreateResult = await CreateUserServiceUser(model, user.Id);
            if (!userServiceCreateResult.IsSuccess)
            {
                _logger.LogError("Не удалось создать пользователя в UserService: {Error}", userServiceCreateResult.ErrorMessage);
                // Здесь можно решить, что делать - откатывать регистрацию или продолжать
            }

            _logger.LogInformation("✅ Пользователь {Email} зарегистрирован", model.Email);

            return Ok(new
            {
                Message = "Пользователь зарегистрирован",
                UserServiceCreated = userServiceCreateResult.IsSuccess
            });
        }

        private string GetDetailedErrorMessage(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateUserName" => "Пользователь с таким email уже существует",
                "DuplicateEmail" => "Пользователь с таким email уже зарегистрирован",
                "InvalidUserName" => "Некорректный формат email",
                "InvalidEmail" => "Некорректный email адрес",
                "PasswordTooShort" => "Пароль должен содержать минимум 6 символов",
                "PasswordRequiresNonAlphanumeric" => "Пароль должен содержать хотя бы один специальный символ (!@#$%^&* и т.д.)",
                "PasswordRequiresDigit" => "Пароль должен содержать хотя бы одну цифру",
                "PasswordRequiresUpper" => "Пароль должен содержать хотя бы одну заглавную букву",
                "PasswordRequiresLower" => "Пароль должен содержать хотя бы одну строчную букву",
                "PasswordTooCommon" => "Пароль слишком простой, выберите более сложный пароль",
                "InvalidPassword" => "Пароль не соответствует требованиям безопасности",
                "InvalidPhoneNumber" => "Некорректный формат номера телефона",
                _ => GetDefaultPasswordRequirements()
            };
        }

        private string GetDefaultPasswordRequirements()
        {
            var options = _userManager.Options.Password;
            var requirements = new List<string>();

            if (options.RequiredLength > 0)
                requirements.Add($"минимум {options.RequiredLength} символов");
            if (options.RequireNonAlphanumeric)
                requirements.Add("специальные символы");
            if (options.RequireDigit)
                requirements.Add("цифры");
            if (options.RequireUppercase)
                requirements.Add("заглавные буквы");
            if (options.RequireLowercase)
                requirements.Add("строчные буквы");

            return requirements.Count > 0
                ? $"Пароль должен содержать: {string.Join(", ", requirements)}"
                : "Пароль не соответствует требованиям";
        }

        private async Task<(bool IsSuccess, string ErrorMessage)> CreateUserServiceUser(RegisterModel model, string identityUserId)
        {
            try
            {
                var userServiceUser = new UserServiceUserModel
                {
                    Id = Guid.Parse(identityUserId),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Patronymic = model.Patronymic,
                    Email = model.Email ?? string.Empty,
                    PhoneNumber = model.PhoneNumber ?? string.Empty,
                    Age = null
                };

                var json = JsonSerializer.Serialize(userServiceUser);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var userServiceUrl = _configuration["UserService:BaseUrl"] ?? "http://hpm-system.userservice:8080";
                var response = await _httpClient.PostAsync($"{userServiceUrl}/api/Users", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Пользователь успешно создан в UserService с ID: {UserId}", identityUserId);
                    return (true, string.Empty);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка при создании пользователя в UserService. Status: {Status}, Content: {Content}",
                        response.StatusCode, errorContent);
                    return (false, $"UserService error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Исключение при создании пользователя в UserService");
                return (false, ex.Message);
            }
        }
    }
}