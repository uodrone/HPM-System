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

        public async Task<List<Guid>> GetHouseOwnerIdsAsync(long houseId, CancellationToken ct = default)
        {
            if (houseId <= 0)
                throw new ArgumentException("Некорректный ID дома", nameof(houseId));

            var requestUrl = $"api/house/{houseId}/owner-ids";

            try
            {
                var response = await _httpClient.GetAsync(requestUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    var userIds = JsonSerializer.Deserialize<List<Guid>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return userIds ?? new List<Guid>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new List<Guid>();

                response.EnsureSuccessStatusCode();
                return new List<Guid>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении владельцев дома {HouseId}", houseId);
                throw;
            }
        }
    }
}
