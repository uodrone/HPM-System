// Middleware/AuthenticationMiddleware.cs
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HPM_System.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Обрабатываем код аутентификации на главной странице
            if (context.Request.Path == "/" && context.Request.Query.ContainsKey("auth"))
            {
                var authCode = context.Request.Query["auth"].ToString();
                await HandleAuthCodeAsync(context, authCode);
            }

            // Проверяем наличие JWT токена для API запросов
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                await ValidateJwtTokenAsync(context);
            }

            await _next(context);
        }

        private async Task HandleAuthCodeAsync(HttpContext context, string authCode)
        {
            try
            {
                _logger.LogInformation("Обработка кода аутентификации: {AuthCode}", authCode);

                // Обмениваем код на токен через IdentityServer
                var httpClient = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
                var identityServerUrl = _configuration["IdentityServer:BaseUrl"] ?? "http://hpm-system.identityserver:8080";

                var requestContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { AuthCode = authCode }),
                    Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    $"{identityServerUrl}/api/Account/exchange-auth-code", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var authData = System.Text.Json.JsonSerializer.Deserialize<AuthResult>(result,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (authData?.Token != null)
                    {
                        // Устанавливаем токен в cookie
                        context.Response.Cookies.Append("auth_token", authData.Token, new CookieOptions
                        {
                            HttpOnly = false, // Нужен доступ из JavaScript
                            Secure = false,   // Для development, в production поставить true
                            SameSite = SameSiteMode.Strict,
                            MaxAge = TimeSpan.FromHours(1)
                        });

                        // Добавляем информацию о пользователе в контекст
                        var claims = ExtractClaimsFromToken(authData.Token);
                        if (claims.Any())
                        {
                            var identity = new ClaimsIdentity(claims, "jwt");
                            context.User = new ClaimsPrincipal(identity);
                        }

                        _logger.LogInformation("✅ Токен успешно установлен для пользователя: {UserId}", authData.UserId);
                    }
                }
                else
                {
                    _logger.LogWarning("❌ Ошибка при обмене кода аутентификации: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Исключение при обработке кода аутентификации");
            }
        }

        private async Task ValidateJwtTokenAsync(HttpContext context)
        {
            var token = ExtractTokenFromRequest(context);

            if (string.IsNullOrEmpty(token))
            {
                return; // Нет токена - продолжаем без аутентификации
            }

            try
            {
                var claims = ExtractClaimsFromToken(token);
                if (claims.Any())
                {
                    var identity = new ClaimsIdentity(claims, "jwt");
                    context.User = new ClaimsPrincipal(identity);
                    _logger.LogDebug("✅ JWT токен валиден для запроса: {Path}", context.Request.Path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Ошибка валидации JWT токена для запроса: {Path}", context.Request.Path);

                // Удаляем невалидный токен
                context.Response.Cookies.Delete("auth_token");
            }
        }

        private string? ExtractTokenFromRequest(HttpContext context)
        {
            // Пытаемся получить токен из заголовка Authorization
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader["Bearer ".Length..].Trim();
            }

            // Пытаемся получить из cookie
            if (context.Request.Cookies.TryGetValue("auth_token", out var cookieToken))
            {
                return cookieToken;
            }

            return null;
        }

        private IEnumerable<Claim> ExtractClaimsFromToken(string token)
        {
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

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal.Claims;
            }
            catch
            {
                return Enumerable.Empty<Claim>();
            }
        }
    }
}