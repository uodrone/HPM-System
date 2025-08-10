// Services/AccountService.cs
using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HPM_System.IdentityServer.Services.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccountService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public async Task<AccountResult<LoginResultData>> LoginAsync(LoginModel model)
        {
            _logger.LogInformation("Попытка входа: {LoginField}", model.EmailOrPhone);

            // Находим пользователя по email или телефону
            var user = await FindUserByEmailOrPhoneAsync(model.EmailOrPhone);

            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден: {LoginField}", model.EmailOrPhone);
                return AccountResult<LoginResultData>.Unauthorized("Неверные учетные данные");
            }

            // Проверяем пароль
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Неверный пароль для пользователя: {UserId}", user.Id);
                return AccountResult<LoginResultData>.Unauthorized("Неверные учетные данные");
            }

            // Генерируем JWT токен
            var token = await GenerateJwtTokenAsync(user);

            _logger.LogInformation("✅ Успешный вход пользователя: {UserId}", user.Id);

            return AccountResult<LoginResultData>.Success(new LoginResultData
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Token = token
            });
        }

        public async Task<AccountResult<RegistrationResultData>> RegisterAsync(RegisterModel model)
        {
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
                _logger.LogWarning("Ошибка регистрации: {Email}", model.Email);
                return AccountResult<RegistrationResultData>.Failure(
                    "Ошибка регистрации пользователя",
                    result.Errors.ToList());
            }

            // Создаем пользователя в UserService
            var userServiceCreateResult = await CreateUserServiceUserAsync(model, user.Id);

            _logger.LogInformation("✅ Пользователь {Email} зарегистрирован", model.Email);

            return AccountResult<RegistrationResultData>.Success(new RegistrationResultData
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                UserServiceCreated = userServiceCreateResult.IsSuccess
            });
        }

        public async Task<IdentityUser?> FindUserByEmailOrPhoneAsync(string emailOrPhone)
        {
            // Сначала пытаемся найти по email
            var userByEmail = await _userManager.FindByEmailAsync(emailOrPhone);
            if (userByEmail != null)
                return userByEmail;

            // Затем ищем по телефону
            var users = _userManager.Users.Where(u => u.PhoneNumber == emailOrPhone);
            return users.FirstOrDefault();
        }

        public async Task<string> GenerateJwtTokenAsync(IdentityUser user)
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

        private async Task<(bool IsSuccess, string ErrorMessage)> CreateUserServiceUserAsync(RegisterModel model, string identityUserId)
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