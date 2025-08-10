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
}