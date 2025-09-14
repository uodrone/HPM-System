using HPM_System.IdentityServer.Models;
using HPM_System.IdentityServer.Services;
using HPM_System.IdentityServer.Services.AccountService;
using HPM_System.IdentityServer.Services.ErrorHandlingService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HPM_System.IdentityServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IAccountService _accountService;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly IConfiguration _configuration;

        public AuthController(
            SignInManager<IdentityUser> signInManager,
            ILogger<AuthController> logger,
            IAccountService accountService,
            IErrorHandlingService errorHandlingService,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _logger = logger;
            _accountService = accountService;
            _errorHandlingService = errorHandlingService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.LoginAsync(model);

            if (result.IsSuccess && result.Data != null)
            {
                var user = await _accountService.FindUserByEmailOrPhoneAsync(model.EmailOrPhone);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, model.RememberMe);
                    return await RedirectAfterAuth(returnUrl, user);
                }
            }

            if (result.IsUnauthorized)
            {
                ModelState.AddModelError(string.Empty, "Неверные учетные данные.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ошибка при входе в систему.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.RegisterAsync(model);

            if (result.IsSuccess && result.Data != null)
            {
                if (!result.Data.UserServiceCreated)
                {
                    TempData["Warning"] = "Аккаунт создан, но произошла ошибка при синхронизации данных.";
                }

                var user = await _accountService.FindUserByEmailOrPhoneAsync(model.Email ?? "");
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return await RedirectAfterAuth(returnUrl, user);
                }
            }

            // Обрабатываем ошибки
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    var errorDetail = _errorHandlingService.GetDetailedErrorMessage(error);
                    ModelState.AddModelError(string.Empty, errorDetail);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ошибка регистрации пользователя");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Пользователь вышел из системы.");
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Определяет URL для редиректа после успешной аутентификации
        /// </summary>
        private async Task<IActionResult> RedirectAfterAuth(string? returnUrl, IdentityUser? user = null)
        {
            var redirectUrl = await GetMainAppUrlWithAuthAsync(returnUrl, user);
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Получает URL главного приложения HPM-System с токеном аутентификации
        /// </summary>
        private async Task<string> GetMainAppUrlWithAuthAsync(string? returnUrl = null, IdentityUser? user = null)
        {
            // Получаем URL главного приложения из конфигурации
            var mainAppUrl = _configuration["AppUrls:MainApp"] ?? "https://localhost:55671";

            string finalUrl;

            // Если указан returnUrl и он разрешен, используем его
            if (!string.IsNullOrEmpty(returnUrl) && IsAllowedReturnUrl(returnUrl))
            {
                _logger.LogInformation("Используется указанный returnUrl: {ReturnUrl}", returnUrl);

                // Если returnUrl уже полный URL, возвращаем как есть
                if (Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
                {
                    finalUrl = returnUrl;
                }
                else
                {
                    // Если это относительный путь, добавляем к основному URL
                    finalUrl = $"{mainAppUrl.TrimEnd('/')}/{returnUrl.TrimStart('/')}";
                }
            }
            else
            {
                finalUrl = mainAppUrl;
            }

            // Генерируем временный токен для передачи информации об аутентификации
            if (user != null)
            {
                var authToken = await _accountService.GenerateJwtTokenAsync(user);

                // Создаем одноразовый код для безопасной передачи
                var authCode = Guid.NewGuid().ToString("N");

                // Сохраняем временно в кеше (можно использовать IMemoryCache)
                var cacheKey = $"auth_code_{authCode}";
                var cache = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                cache.Set(cacheKey, new AuthTransferData
                {
                    Token = authToken,
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? ""
                }, TimeSpan.FromMinutes(2)); // Короткое время жизни

                // Добавляем код к URL
                var separator = finalUrl.Contains('?') ? "&" : "?";
                finalUrl = $"{finalUrl}{separator}auth={authCode}";

                _logger.LogInformation("Создан временный код аутентификации: {AuthCode}", authCode);
            }

            _logger.LogInformation("Финальный URL для редиректа: {FinalUrl}", finalUrl);
            return finalUrl;
        }

        /// <summary>
        /// Проверяет, разрешен ли указанный returnUrl для безопасности
        /// </summary>
        private bool IsAllowedReturnUrl(string returnUrl)
        {
            var allowedHosts = _configuration.GetSection("AllowedRedirectHosts").Get<string[]>()
                ?? new[] { "localhost:55671", "localhost:55670", "localhost:55675", "localhost:55676" };

            // Проверяем локальные пути (относительные URL в рамках IdentityServer)
            if (Url.IsLocalUrl(returnUrl))
            {
                return true;
            }

            // Проверяем разрешенные внешние хосты
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            {
                var hostWithPort = $"{uri.Host}:{uri.Port}";
                var isAllowed = allowedHosts.Any(host =>
                    string.Equals(host, hostWithPort, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(host, uri.Host, StringComparison.OrdinalIgnoreCase));

                _logger.LogInformation("Проверка разрешенного хоста {Host}: {IsAllowed}", hostWithPort, isAllowed);
                return isAllowed;
            }

            _logger.LogWarning("Неверный формат returnUrl: {ReturnUrl}", returnUrl);
            return false;
        }
    }
}