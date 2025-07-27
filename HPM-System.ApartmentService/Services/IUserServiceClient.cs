// Services/IUserServiceClient.cs
using HPM_System.ApartmentService.DTOs;

namespace HPM_System.ApartmentService.Services
{
    public interface IUserServiceClient
    {
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByPhoneAsync(string phone);
        Task<bool> UserExistsAsync(int userId);
    }
}