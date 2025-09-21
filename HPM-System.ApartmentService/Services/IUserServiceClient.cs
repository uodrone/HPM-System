// Services/IUserServiceClient.cs
using DTOs.UserDTOs;

namespace HPM_System.ApartmentService.Services
{
    public interface IUserServiceClient
    {
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> GetUserByPhoneAsync(string phone);
        Task<bool> UserExistsAsync(Guid userId);
    }
}