using HPM_System.EventService.DTOs;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace HPM_System.EventService.Services.InterfacesImplementation
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;
        private readonly string _userServiceBaseUrl;
        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userServiceBaseUrl = configuration["Services:UserService:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:55680";
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            if (userId == default)
            {
                _logger.LogWarning("Некорректный ID пользователя: {UserId}", userId);
                return null;
            }

            string requestUrl = $"{_userServiceBaseUrl}/api/Users/{userId}";
            try
            {
                _logger.LogDebug("Отправка запроса на получение пользователя по ID: {Url}", requestUrl);
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Получен успешный ответ для пользователя ID {UserId}. Длина контента: {Length} символов.", userId, jsonContent.Length);
                    
                    return JsonSerializer.Deserialize<UserDTO>(
                        jsonContent, 
                        new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Пользователь с ID {UserId} не найден (404).", userId);
                    return null;
                }

                _logger.LogWarning("Неуспешный HTTP статус при получении пользователя {UserId}: {StatusCode} {ReasonPhrase}", userId, response.StatusCode, response.ReasonPhrase);
                response.EnsureSuccessStatusCode();
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP ошибка при получении пользователя {UserId} по URL {Url}", userId, requestUrl);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут или отмена запроса при получении пользователя {UserId} по URL {Url}", userId, requestUrl);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации JSON при получении пользователя {UserId} по URL {Url}", userId, requestUrl);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданное исключение при получении пользователя {UserId} по URL {Url}", userId, requestUrl);
                throw;
            }
        }

        public async Task<IEnumerable<UserDTO>?> GetAllUsersAsync()
        {
            string requestUrl = $"{_userServiceBaseUrl}/api/Users/";
            try
            {
                _logger.LogDebug("Отправка запроса на получение пользователей по ID: {Url}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Получен успешный ответ пользователей. Длина контента: {Length} символов.", jsonContent.Length);

                    return JsonSerializer.Deserialize<IEnumerable<UserDTO>>(
                        jsonContent,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                _logger.LogWarning("Неуспешный HTTP статус при получении пользователей: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                response.EnsureSuccessStatusCode();
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP ошибка при получении пользователей по URL {Url}", requestUrl);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут или отмена запроса при получении пользователей по URL {Url}", requestUrl);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации JSON при получении пользователей  по URL {Url}", requestUrl);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданное исключение при получении пользователей  по URL {Url}", requestUrl);
                throw;
            }
        }
    }
}
