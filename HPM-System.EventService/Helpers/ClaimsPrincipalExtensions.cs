using System.Security.Claims;

namespace HPM_System.EventService.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst("sub")?.Value
                         ?? principal.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            }

            return userId;
        }

        public static string? GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst("email")?.Value;
        }

        public static string? GetUserPhone(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("phone_number")?.Value
                ?? principal.FindFirst(ClaimTypes.MobilePhone)?.Value;
        }
    }
}