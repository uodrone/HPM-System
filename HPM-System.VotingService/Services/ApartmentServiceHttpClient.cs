using DTO;
using System.Text.Json;

namespace VotingService.Services;

public class ApartmentServiceHttpClient : IApartmentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApartmentServiceHttpClient> _logger;

    public ApartmentServiceHttpClient(HttpClient httpClient, ILogger<ApartmentServiceHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ApartmentResponseDto>> GetApartmentsByHouseIdAsync(long houseId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/house/{houseId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apartments = JsonSerializer.Deserialize<List<ApartmentResponseDto>>(json, options)
                              ?? new List<ApartmentResponseDto>();
            return apartments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при вызове ApartmentService для houseId {HouseId}", houseId);
            throw; // ошибка будет обработана в контроллере
        }
    }
}