using HPM_System.EventService.DTOs;

namespace HPM_System.EventService.Services.Interfaces
{
    public interface IUserServiceClient
    {
        Task<UserDTO?> GetUserByIdAsync(Guid userId);
    }
}
