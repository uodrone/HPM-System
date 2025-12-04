using System.Text.Json;
using HPM_System.TelegramBotService.DTO;

namespace HPM_System.TelegramBotService.Services;

public class UserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Guid?> GetUserIdByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            // Кодируем номер телефона для URL
            var encodedPhone = Uri.EscapeDataString(phoneNumber);
            var response = await _httpClient.GetAsync($"/api/Users/by-phone/{encodedPhone}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Пользователь с номером {Phone} не найден в системе. Статус: {StatusCode}",
                    phoneNumber, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var user = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении userId по номеру телефона {Phone}", phoneNumber);
            return null;
        }
    }
}