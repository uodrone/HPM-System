using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HPM_System.IdentityServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Попытка входа: {LoginField}", model.EmailOrPhone);

            // Находим пользователя по email или телефону
            var user = await FindUserByEmailOrPhone(model.EmailOrPhone);

            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден: {LoginField}", model.EmailOrPhone);
                return BadRequest(new { Message = "Неверные учетные данные" });
            }

            // Проверяем пароль
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Неверный пароль для пользователя: {UserId}", user.Id);
                return BadRequest(new { Message = "Неверные учетные данные" });
            }

            // Генерируем JWT токен
            var token = await GenerateJwtToken(user);

            _logger.LogInformation("✅ Успешный вход пользователя: {UserId}", user.Id);

            return Ok(new
            {
                Message = "Успешный вход",
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
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
            }

            _logger.LogInformation("✅ Пользователь {Email} зарегистрирован", model.Email);

            return Ok(new
            {
                Message = "Пользователь зарегистрирован",
                UserServiceCreated = userServiceCreateResult.IsSuccess,
                UserId = user.Id
            });
        }

        private async Task<IdentityUser?> FindUserByEmailOrPhone(string emailOrPhone)
        {
            // Сначала пытаемся найти по email
            var userByEmail = await _userManager.FindByEmailAsync(emailOrPhone);
            if (userByEmail != null)
                return userByEmail;

            // Затем ищем по телефону
            var users = _userManager.Users.Where(u => u.PhoneNumber == emailOrPhone);
            return users.FirstOrDefault();
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-very-long-secret-key-here-minimum-256-bits";
            var issuer = jwtSettings["Issuer"] ?? "HPM_System.IdentityServer";
            var audience = jwtSettings["Audience"] ?? "HPM_System.Clients";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("phone_number", user.PhoneNumber ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetDetailedErrorMessage(IdentityError error)
        {
            return error.Code switch
            {
                "DuplicateUserName" => "Пользователь с таким email уже существует",
                "DuplicateEmail" => "Пользователь с таким email уже зарегистрирован",
                "InvalidUserName" => "Некорректный формат email",
                "InvalidEmail" => "Некорректный email адрес",
                "PasswordTooShort" => "Пароль должен содержать минимум 8 символов",
                "PasswordRequiresNonAlphanumeric" => "Пароль должен содержать хотя бы один специальный символ",
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