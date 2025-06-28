using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HPM_System.IdentityServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
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

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Ошибка регистрации: {Code} — {Description}",
                        error.Code, error.Description);
                }

                return BadRequest(result.Errors);
            }

            _logger.LogInformation("✅ Пользователь {Email} зарегистрирован", model.Email);

            return Ok(new { Message = "Пользователь зарегистрирован" });
        }
    }
}
