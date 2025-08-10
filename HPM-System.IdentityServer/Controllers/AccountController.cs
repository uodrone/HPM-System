using HPM_System.IdentityServer.Services.AccountService;
using HPM_System.IdentityServer.Models;
using HPM_System.IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using HPM_System.IdentityServer.Services.ErrorHandlingService;

namespace HPM_System.IdentityServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;
        private readonly IErrorHandlingService _errorHandlingService;

        public AccountController(
            ILogger<AccountController> logger,
            IAccountService accountService,
            IErrorHandlingService errorHandlingService)
        {
            _logger = logger;
            _accountService = accountService;
            _errorHandlingService = errorHandlingService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.LoginAsync(model);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(new
                {
                    Message = "Успешный вход",
                    Token = result.Data.Token,
                    UserId = result.Data.UserId,
                    Email = result.Data.Email,
                    PhoneNumber = result.Data.PhoneNumber
                });
            }

            if (result.IsUnauthorized)
            {
                return Unauthorized(new { Message = result.ErrorMessage ?? "Неверные учетные данные" });
            }

            return BadRequest(new { Message = result.ErrorMessage ?? "Ошибка при входе" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.RegisterAsync(model);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(new
                {
                    Message = "Пользователь зарегистрирован",
                    UserServiceCreated = result.Data.UserServiceCreated,
                    UserId = result.Data.UserId
                });
            }

            // Формируем детальные ошибки для API ответа
            var errorDetails = new List<object>();

            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    var errorDetail = _errorHandlingService.GetDetailedErrorMessage(error);
                    errorDetails.Add(new
                    {
                        Code = error.Code,
                        Description = error.Description,
                        DetailedMessage = errorDetail
                    });
                }
            }

            return BadRequest(new
            {
                Message = result.ErrorMessage ?? "Ошибка регистрации пользователя",
                Errors = errorDetails
            });
        }
    }
}