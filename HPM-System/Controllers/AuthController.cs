// Controllers/AuthController.cs - для сервиса hpm-system
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HPM_System.Models;

namespace HPM_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly HttpClient _httpClient;

        public AuthController(
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Обменивает код аутентификации на токен из IdentityServer
        /// </summary>
        [HttpPost("exchange-code")]
        public async Task<IActionResult> ExchangeAuthCode([FromBody] AuthCodeModel model)
        {
            try
            {
                var baseUrlFromConfig = _configuration["IdentityServer:BaseUrl"];
                var authorityFromConfig = _configuration["IdentityServer:Authority"];

                _logger.LogInformation("IdentityServer:BaseUrl = {BaseUrl}", baseUrlFromConfig);
                _logger.LogInformation("IdentityServer:Authority = {Authority}", authorityFromConfig);

                var identityServerUrl = baseUrlFromConfig ?? "http://hpm-system.identityserver:8080";
                _logger.LogInformation("Используемый URL: {Url}", identityServerUrl);

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { AuthCode = model.AuthCode }),
                    Encoding.UTF8, "application/json");

                var fullUrl = $"{identityServerUrl}/api/Account/exchange-auth-code";
                _logger.LogInformation("Полный URL запроса: {FullUrl}", fullUrl);

                var response = await _httpClient.PostAsync(fullUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var authData = System.Text.Json.JsonSerializer.Deserialize<AuthResultModel>(result,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Валидируем полученный JWT токен
                    var validationResult = ValidateJwtToken(authData?.Token);
                    if (validationResult.IsValid)
                    {
                        return Ok(new
                        {
                            Message = "Авторизация успешна",
                            Token = authData?.Token,
                            UserId = authData?.UserId,
                            Email = authData?.Email,
                            PhoneNumber = authData?.PhoneNumber,
                            IsAuthenticated = true
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Получен невалидный JWT токен");
                        return BadRequest(new { Message = "Получен невалидный токен", IsAuthenticated = false });
                    }
                }

                _logger.LogWarning("Ошибка при обмене кода аутентификации: {StatusCode}", response.StatusCode);
                return BadRequest(new { Message = "Недействительный код аутентификации", IsAuthenticated = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обмене кода аутентификации");
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера", IsAuthenticated = false });
            }
        }

        /// <summary>
        /// Валидирует JWT токен
        /// </summary>
        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] TokenValidationModel model)
        {
            var result = ValidateJwtToken(model.Token);

            if (result.IsValid && result.Claims != null)
            {
                return Ok(new
                {
                    IsAuthenticated = true,
                    UserId = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                    Email = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    PhoneNumber = result.Claims.FirstOrDefault(c => c.Type == "phone_number")?.Value,
                    ExpiresAt = result.ExpiresAt
                });
            }

            return Ok(new { IsAuthenticated = false });
        }

        /// <summary>
        /// Получает текущую информацию об аутентификации из токена
        /// </summary>
        [HttpGet("current-user")]
        public IActionResult GetCurrentUser()
        {
            var token = ExtractTokenFromRequest();
            if (string.IsNullOrEmpty(token))
            {
                return Ok(new { IsAuthenticated = false });
            }

            var result = ValidateJwtToken(token);

            if (result.IsValid && result.Claims != null)
            {
                return Ok(new
                {
                    IsAuthenticated = true,
                    UserId = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                    Email = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    PhoneNumber = result.Claims.FirstOrDefault(c => c.Type == "phone_number")?.Value,
                    ExpiresAt = result.ExpiresAt
                });
            }

            return Ok(new { IsAuthenticated = false });
        }

        private Models.TokenValidationResult ValidateJwtToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new Models.TokenValidationResult { IsValid = false };
            }

            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? "your-very-long-secret-key-here-minimum-256-bits";
                var issuer = jwtSettings["Issuer"] ?? "HPM_System.IdentityServer";
                var audience = jwtSettings["Audience"] ?? "HPM_System.Clients";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                return new Models.TokenValidationResult
                {
                    IsValid = true,
                    Claims = principal.Claims,
                    ExpiresAt = jwtToken?.ValidTo
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации JWT токена");
                return new Models.TokenValidationResult { IsValid = false };
            }
        }

        private string? ExtractTokenFromRequest()
        {
            // Попробуем извлечь токен из заголовка Authorization
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader["Bearer ".Length..].Trim();
            }

            // Или из cookie
            if (Request.Cookies.TryGetValue("auth_token", out var cookieToken))
            {
                return cookieToken;
            }

            return null;
        }
    }
}