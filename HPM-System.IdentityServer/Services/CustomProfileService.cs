using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HPM_System.IdentityServer.Services
{
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomProfileService> _logger;

        public CustomProfileService(
            UserManager<ApplicationUser> userManager,
            ILogger<CustomProfileService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            _logger.LogInformation("Получение данных профиля для пользователя: {Subject}", context.Subject.Identity.Name);

            var user = await _userManager.GetUserAsync(context.Subject);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim("email", user.Email ?? string.Empty),
                    new Claim("given_name", user.FirstName ?? string.Empty),
                    new Claim("family_name", user.LastName ?? string.Empty),
                    new Claim("phone_number", user.PhoneNumber ?? string.Empty),
                    new Claim("sub", user.Id), // Subject ID
                    // Добавляем стандартные claims
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                    new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
                };

                // Добавляем отчество если есть
                if (!string.IsNullOrEmpty(user.Patronymic))
                {
                    claims.Add(new Claim("patronymic", user.Patronymic));
                }

                // Добавляем все claims без фильтрации для отладки
                context.IssuedClaims.AddRange(claims);

                _logger.LogInformation("Добавлено {Count} claims для пользователя {Email}",
                    claims.Count, user.Email);

                // Логируем все добавленные claims
                foreach (var claim in claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
            }
            else
            {
                _logger.LogWarning("Пользователь не найден для Subject: {Subject}", context.Subject.Identity.Name);
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            context.IsActive = user != null;

            _logger.LogInformation("Проверка активности пользователя: {Subject}, активен: {IsActive}",
                context.Subject.Identity.Name, context.IsActive);
        }
    }
}