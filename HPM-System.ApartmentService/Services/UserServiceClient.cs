using HPM_System.ApartmentService.DTOs;
using System.Text.Json;
using System.Net;

namespace HPM_System.ApartmentService.Services
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
            _userServiceBaseUrl = configuration["Services:UserService:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:55680";
        }

        /// <inheritdoc />
        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некорректный ID пользователя: {UserId}", userId);
                // Можно выбросить ArgumentException, если это предпочтительнее
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
                    // Предполагаем, что UserDto соответствует структуре, возвращаемой UserService
                    return JsonSerializer.Deserialize<UserDto>(jsonContent, new JsonSerializerOptions
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
                response.EnsureSuccessStatusCode(); // Это выбросит HttpRequestException
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

        public async Task<UserDto?> GetUserByPhoneAsync(string phone)
        {
            // Валидация входных данных
            if (string.IsNullOrWhiteSpace(phone))
            {
                _logger.LogWarning("Получен пустой или null номер телефона.");
                // Можно выбросить ArgumentException, если это предпочтительнее
                return null;
            }

            string encodedPhone = Uri.EscapeDataString(phone);
            string requestUrl = $"{_userServiceBaseUrl}/api/Users/by-phone/{encodedPhone}";

            try
            {
                _logger.LogDebug("Отправка запроса на получение пользователя по номеру телефона: {Url}", requestUrl);
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Получен успешный ответ для пользователя с телефоном {Phone}. Длина контента: {Length} символов.", phone, jsonContent.Length);
                    // Предполагаем, что UserDto соответствует структуре, возвращаемой UserService
                    return JsonSerializer.Deserialize<UserDto>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Пользователь с телефоном {Phone} не найден (404).", phone);
                    return null;
                }

                // Для других HTTP ошибок (4xx, 5xx) логируем и выбрасываем исключение
                _logger.LogWarning("Неуспешный HTTP статус при получении пользователя по телефону {Phone}: {StatusCode} {ReasonPhrase}", phone, response.StatusCode, response.ReasonPhrase);
                response.EnsureSuccessStatusCode(); // Это выбросит HttpRequestException
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP ошибка при получении пользователя по телефону {Phone} по URL {Url}", phone, requestUrl);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут или отмена запроса при получении пользователя по телефону {Phone} по URL {Url}", phone, requestUrl);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка десериализации JSON при получении пользователя по телефону {Phone} по URL {Url}", phone, requestUrl);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданное исключение при получении пользователя по телефону {Phone} по URL {Url}", phone, requestUrl);
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некорректный ID пользователя для проверки существования: {UserId}", userId);
                // Можно выбросить ArgumentException, если это предпочтительнее
                return false;
            }

            string requestUrl = $"{_userServiceBaseUrl}/api/Users/{userId}";
            try
            {
                _logger.LogDebug("Отправка запроса на проверку существования пользователя ID: {Url}", requestUrl);
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Пользователь с ID {UserId} существует (2xx).", userId);
                    return true;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Пользователь с ID {UserId} не существует (404).", userId);
                    return false;
                }

                _logger.LogWarning("Неуспешный HTTP статус при проверке существования пользователя {UserId}: {StatusCode} {ReasonPhrase}. Считаем, что проверка не удалась, но это не сетевая ошибка.", userId, response.StatusCode, response.ReasonPhrase);
                return false;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP ошибка при проверке существования пользователя {UserId} по URL {Url}. Проблема с сетью или сервер недоступен.", userId, requestUrl);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Таймаут или отмена запроса при проверке существования пользователя {UserId} по URL {Url}. Проблема с сетью или сервер не отвечает.", userId, requestUrl);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданное исключение при проверке существования пользователя {UserId} по URL {Url}", userId, requestUrl);
                throw;
            }
        }
    }
}