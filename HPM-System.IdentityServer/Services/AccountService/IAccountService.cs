// Services/IAccountService.cs
using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace HPM_System.IdentityServer.Services.AccountService
{
    public interface IAccountService
    {
        Task<AccountResult<LoginResultData>> LoginAsync(LoginModel model);
        Task<AccountResult<RegistrationResultData>> RegisterAsync(RegisterModel model);
        Task<IdentityUser?> FindUserByEmailOrPhoneAsync(string emailOrPhone);
        Task<string> GenerateJwtTokenAsync(IdentityUser user);
    }

    public class AccountResult<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsUnauthorized { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public List<IdentityError>? Errors { get; set; }

        public static AccountResult<T> Success(T data)
        {
            return new AccountResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        public static AccountResult<T> Failure(string errorMessage, List<IdentityError>? errors = null)
        {
            return new AccountResult<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Errors = errors
            };
        }

        public static AccountResult<T> Unauthorized(string errorMessage)
        {
            return new AccountResult<T>
            {
                IsSuccess = false,
                IsUnauthorized = true,
                ErrorMessage = errorMessage
            };
        }
    }

    public class LoginResultData
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class RegistrationResultData
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool UserServiceCreated { get; set; }
    }
}