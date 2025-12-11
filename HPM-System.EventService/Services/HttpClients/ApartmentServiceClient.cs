using HPM_System.EventService.Interfaces;
using System.Text.Json;

namespace HPM_System.EventService.Services.HttpClients
{
    public class ApartmentServiceClient : IApartmentServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApartmentServiceClient> _logger;

        public ApartmentServiceClient(HttpClient httpClient, ILogger<ApartmentServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Guid>> GetHouseUserIdsAsync(long houseId, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/apartment/house/{houseId}/user-ids", ct);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Дом не найден → возвращаем пустой список
                    return new List<Guid>();
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var userIds = JsonSerializer.Deserialize<List<Guid>>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return userIds ?? new List<Guid>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при вызове ApartmentService для получения user-ids дома {HouseId}", houseId);
                throw; // или return new List<Guid>() — по политике отработки ошибок
            }
        }
    }
}