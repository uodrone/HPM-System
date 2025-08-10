using HPM_System.NotificationService.Application.DTO;

namespace HPM_System.NotificationService.Application.Interfaces
{
    public interface IRabbitMQHandler
    {
        string QueueName { get; }
        string RoutingKeyPattern { get; }

        public Task ExecuteAsync(RabbitMQDTO dto);
    }
}
