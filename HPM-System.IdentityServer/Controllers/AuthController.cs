using HPM_System.IdentityServer.Services.AccountService;
using HPM_System.IdentityServer.Models;
using HPM_System.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HPM_System.IdentityServer.Services.ErrorHandlingService;

namespace HPM_System.IdentityServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IAccountService _accountService;
        private readonly IErrorHandlingService _errorHandlingService;

        public AuthController(
            SignInManager<IdentityUser> signInManager,
            ILogger<AuthController> logger,
            IAccountService accountService,
            IErrorHandlingService errorHandlingService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _accountService = accountService;
            _errorHandlingService = errorHandlingService;
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
                    return RedirectToLocal(returnUrl);
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
                    return RedirectToLocal(returnUrl);
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

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}