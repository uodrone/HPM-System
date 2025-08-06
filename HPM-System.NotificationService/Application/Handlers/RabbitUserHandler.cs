using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.DTO.Rabbit;
using HPM_System.NotificationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HPM_System.NotificationService.Application.Handlers
{    
    public class RabbitUserHandler : IRabbitUserHandler
    {
        private INotificationAppService _notificationAppService;
        private readonly Dictionary<string, Func<string, Task>> _handlers;

        public RabbitUserHandler(INotificationAppService notificationAppService)
        {
            _notificationAppService = notificationAppService;

            _handlers = new()
            {
                ["notification.user.registered"] = HandleRegisteredAsync                
            };
        }

        public async Task ExecuteAsync(RabbitDTO dto)
        {
            if (!_handlers.TryGetValue(dto.RoutingKey, out var handler))
            {
                Console.WriteLine($"[Handler] Unknown routing key: {dto.RoutingKey}");
                throw new InvalidOperationException($"Unsupported routing key: {dto.RoutingKey}");
            }

            await handler(dto.Payload);
        }

        private async Task HandleRegisteredAsync(string payload)
        {
            var data = JsonSerializer.Deserialize<UserRegisteredDTO>(payload) ?? throw new InvalidOperationException("Data is empty");

            if (data.Id == Guid.Empty)
                throw new InvalidOperationException("Id is required!");

            if (string.IsNullOrWhiteSpace(data.Name))
                throw new InvalidOperationException("Name is required!");

            await _notificationAppService.CreateNotificationAsync(new DTO.CreateNotificationDTO
            {
                Title = "Зарегистрирован новый пользователь!",
                Message = $"У нас в системе зарегистрирован новый пользователь с именем {data.Name} и ID {data.Id}",
                CreatedBy = Guid.NewGuid()
            });            
        }
    }
}
