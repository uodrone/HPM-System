using Microsoft.AspNetCore.Identity;

namespace HPM_System.IdentityServer.Services.AccountService
{
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
}
