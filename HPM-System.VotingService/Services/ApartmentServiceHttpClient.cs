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
            // ИСПРАВЛЕНО: правильный путь к эндпоинту
            var response = await _httpClient.GetAsync($"/api/apartment/house/{houseId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // Для отладки
            _logger.LogInformation("Получен ответ от ApartmentService для houseId {HouseId}: {Json}", houseId, json);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apartments = JsonSerializer.Deserialize<List<ApartmentResponseDto>>(json, options)
                              ?? new List<ApartmentResponseDto>();

            return apartments;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при вызове ApartmentService для houseId {HouseId}", houseId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Ошибка десериализации ответа от ApartmentService для houseId {HouseId}", houseId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при вызове ApartmentService для houseId {HouseId}", houseId);
            throw;
        }
    }
}