using HPM_System.NotificationService.Application.DTO;

namespace HPM_System.NotificationService.Application.Interfaces
{
    public interface IRabbitUserHandler
    {
        Task ExecuteAsync(RabbitDTO dto);
    }
}
