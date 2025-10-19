// Services/IUserServiceClient.cs
using DTOs.UserDTOs;

namespace Interfaces
{
    public interface IUserServiceClient
    {
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> GetUserByPhoneAsync(string phone);
        Task<bool> UserExistsAsync(Guid userId);
    }
}