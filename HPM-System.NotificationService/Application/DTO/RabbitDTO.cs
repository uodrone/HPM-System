namespace HPM_System.NotificationService.Application.DTO
{
    public class RabbitDTO
    {
        public string RoutingKey { get; set; } = null!;
        public string Payload { get; set; } = null!;
    }
}
