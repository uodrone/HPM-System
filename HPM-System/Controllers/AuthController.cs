using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace HPM_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] TokenLoginModel model)
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
                var userId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                return Ok(new
                {
                    Message = "Авторизация успешна",
                    User = new
                    {
                        Id = userId,
                        Email = userEmail
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Не удалось обработать токен", Details = ex.Message });
            }
        }
    }

    public class TokenLoginModel
    {
        public string AccessToken { get; set; } = "";
    }
}