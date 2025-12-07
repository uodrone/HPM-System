using HPM_System.EventService.DTOs;
using HPM_System.EventService.Interfaces;
using System.Text;
using System.Text.Json;

namespace HPM_System.EventService.Services.HttpClients
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CreateAsync(CreateEventNotificationRequest request, CancellationToken ct = default)
        {
            if (request.UserIdList == null || !request.UserIdList.Any())
                return; // нечего отправлять

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/notifications", content, ct);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось отправить уведомление о событии");
            }
        }
    }
}
