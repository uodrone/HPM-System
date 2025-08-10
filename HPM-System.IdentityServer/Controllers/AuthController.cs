using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace HPM_System.IdentityServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AuthController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
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

            // Находим пользователя по email или телефону
            var user = await FindUserByEmailOrPhone(model.EmailOrPhone);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверные учетные данные.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Пользователь вошел в систему.");
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Аккаунт пользователя заблокирован.");
                ModelState.AddModelError(string.Empty, "Аккаунт заблокирован.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неверные учетные данные.");
                return View(model);
            }
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

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Пользователь создал новый аккаунт с паролем.");

                // Создаем пользователя в UserService
                var userServiceCreateResult = await CreateUserServiceUser(model, user.Id);
                if (!userServiceCreateResult.IsSuccess)
                {
                    _logger.LogError("Не удалось создать пользователя в UserService: {Error}", userServiceCreateResult.ErrorMessage);
                    TempData["Warning"] = "Аккаунт создан, но произошла ошибка при синхронизации данных.";
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, GetDetailedErrorMessage(error));
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

        private async Task<IdentityUser?> FindUserByEmailOrPhone(string emailOrPhone)
        {
            var userByEmail = await _userManager.FindByEmailAsync(emailOrPhone);
            if (userByEmail != null)
                return userByEmail;

            var users = _userManager.Users.Where(u => u.PhoneNumber == emailOrPhone);
            return users.FirstOrDefault();
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
                _ => error.Description
            };
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
                    return (true, string.Empty);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (false, $"UserService error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}