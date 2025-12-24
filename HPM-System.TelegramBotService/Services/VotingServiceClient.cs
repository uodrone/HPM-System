using HPM_System.TelegramBotService.DTO;
using System.Text;
using System.Text.Json;

namespace HPM_System.TelegramBotService.Services;

public class VotingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VotingServiceClient> _logger;

    public VotingServiceClient(HttpClient httpClient, ILogger<VotingServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<VotingDto?> GetVotingByIdAsync(Guid votingId, CancellationToken cancellationToken = default)
    {
        try
        {
            // ВАЖНО: Этот эндпоинт должен быть без авторизации или с служебным токеном
            var response = await _httpClient.GetAsync($"/api/internal/votings/{votingId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Голосование {VotingId} не найдено. Статус: {StatusCode}",
                    votingId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var voting = JsonSerializer.Deserialize<VotingDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return voting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении голосования {VotingId}", votingId);
            return null;
        }
    }

    public async Task<VoteResult> SubmitVoteAsync(
        Guid votingId,
        Guid userId,
        string response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var voteRequest = new
            {
                userId,
                response
            };

            var json = JsonSerializer.Serialize(voteRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Используем новый эндпоинт для голосования от Telegram
            var httpResponse = await _httpClient.PostAsync(
                $"/api/internal/votings/{votingId}/vote-telegram",
                content,
                cancellationToken);

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Голос успешно отправлен для пользователя {UserId} в голосовании {VotingId}",
                    userId, votingId);
                return new VoteResult { Success = true };
            }

            // Проверяем статус 409 - пользователь уже голосовал
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var errorJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                var errorData = JsonSerializer.Deserialize<AlreadyVotedResponse>(errorJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Пользователь {UserId} уже голосовал в голосовании {VotingId}. Предыдущий выбор: {PreviousResponse}",
                    userId, votingId, errorData?.PreviousResponse);

                return new VoteResult
                {
                    Success = false,
                    AlreadyVoted = true,
                    PreviousResponse = errorData?.PreviousResponse,
                    Message = errorData?.Message
                };
            }

            var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Не удалось отправить голос. Статус: {StatusCode}, Ошибка: {Error}",
                httpResponse.StatusCode, errorContent);

            return new VoteResult { Success = false, Message = errorContent };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке голоса для пользователя {UserId} в голосовании {VotingId}",
                userId, votingId);
            return new VoteResult { Success = false, Message = ex.Message };
        }
    }
}