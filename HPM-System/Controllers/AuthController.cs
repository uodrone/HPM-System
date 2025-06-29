using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using HPM_System.Data;
using Microsoft.EntityFrameworkCore;
using HPM_System.Models.Identity;

namespace HPM_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] TokenLoginModel model)
        {
            if (string.IsNullOrEmpty(model.AccessToken))
                return BadRequest(new { Error = "Токен отсутствует" });

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(model.AccessToken))
                return BadRequest(new { Error = "Неверный формат токена" });

            try
            {
                var token = handler.ReadJwtToken(model.AccessToken);
                var userEmail = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { Error = "Email не найден в токене" });
                }

                // Проверяем существование пользователя в базе данных
                var existingUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (existingUser != null)
                {
                    // Пользователь найден в базе данных - авторизуем
                    return Ok(new
                    {
                        Message = "Авторизация успешна",
                        User = new
                        {
                            Id = existingUser.Id,
                            Email = existingUser.Email,
                            FirstName = existingUser.FirstName,
                            LastName = existingUser.LastName,
                            Patronymic = existingUser.Patronymic,
                            PhoneNumber = existingUser.PhoneNumber,
                            Age = existingUser.Age,
                        }
                    });
                }
                else
                {
                    // Пользователь не найден в базе данных
                    return Unauthorized(new
                    {
                        Message = "Авторизация не удалась. Пользователь не найден в системе",
                        User = new
                        {
                            Email = userEmail
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Не удалось обработать токен", Details = ex.Message });
            }
        }
    }
}