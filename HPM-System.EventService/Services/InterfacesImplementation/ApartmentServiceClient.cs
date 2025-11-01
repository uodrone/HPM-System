using HPM_System.EventService.DTOs;
using HPM_System.EventService.Services.Interfaces;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace HPM_System.EventService.Services.InterfacesImplementation
{
    public class ApartmentServiceClient : IApartmentServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApartmentServiceClient> _logger;
        private readonly string _apartmentServiceBaseUrl;
        public ApartmentServiceClient(
            HttpClient httpClient,
            ILogger<ApartmentServiceClient> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apartmentServiceBaseUrl = configuration["Services:ApartmentService:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:55682";
        }

        public async Task<ApartmentDTO?> GetApartmentByIdAsync(long apartmentID)
        {
            if (apartmentID == default)
            {
                _logger.LogWarning("Некорректный ID дома: {apartmentID}", apartmentID);
                return null;
            }

            string requestUrl = $"{_apartmentServiceBaseUrl}/api/Apartment/{apartmentID}";
            try
            {
                _logger.LogDebug("Отправка запроса на получение дома по ID: {Url}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Получен успешный ответ для дома ID {apartmentID}. Длина контента: {Length} символов.", apartmentID, jsonContent.Length);

                    return JsonSerializer.Deserialize<ApartmentDTO>(
                        jsonContent,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Дом с ID {UserId} не найден (404).", apartmentID);
                    return null;
                }

                _logger.LogWarning("Неуспешный HTTP статус при получении дома {apartmentID}: {StatusCode} {ReasonPhrase}", apartmentID, response.StatusCode, response.ReasonPhrase);
                response.EnsureSuccessStatusCode();
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP ошибка при получении дома {apartmentID} по URL {Url}", apartmentID, requestUrl);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут или отмена запроса при получении дома {apartmentID} по URL {Url}", apartmentID, requestUrl);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации JSON при получении дома {apartmentID} по URL {Url}", apartmentID, requestUrl);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданное исключение при получении дома {apartmentID} по URL {Url}", apartmentID, requestUrl);
                throw;
            }
        }
    }
}
