using Microsoft.AspNetCore.Identity;

namespace HPM_System.IdentityServer.Services.ErrorHandlingService
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ErrorHandlingService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string GetDetailedErrorMessage(IdentityError error)
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
                _ => GetDefaultPasswordRequirements()
            };
        }

        public string GetDefaultPasswordRequirements()
        {
            var options = _userManager.Options.Password;
            var requirements = new List<string>();

            if (options.RequiredLength > 0)
                requirements.Add($"минимум {options.RequiredLength} символов");
            if (options.RequireNonAlphanumeric)
                requirements.Add("специальные символы");
            if (options.RequireDigit)
                requirements.Add("цифры");
            if (options.RequireUppercase)
                requirements.Add("заглавные буквы");
            if (options.RequireLowercase)
                requirements.Add("строчные буквы");

            return requirements.Count > 0
                ? $"Пароль должен содержать: {string.Join(", ", requirements)}"
                : "Пароль не соответствует требованиям";
        }
    }
}
