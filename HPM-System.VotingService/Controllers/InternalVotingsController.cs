using DTO;
using Microsoft.AspNetCore.Mvc;
using VotingService.Services;
using HPM_System.VotingService.CustomExceptions;

namespace VotingService.Controllers;

/// <summary>
/// Внутренний API для взаимодействия между микросервисами
/// БЕЗ авторизации - используется только внутри Docker сети
/// </summary>
[ApiController]
[Route("api/internal/votings")]
public class InternalVotingsController : ControllerBase
{
    private readonly IVotingService _votingService;
    private readonly ILogger<InternalVotingsController> _logger;

    public InternalVotingsController(IVotingService votingService, ILogger<InternalVotingsController> logger)
    {
        _votingService = votingService;
        _logger = logger;
    }

    /// <summary>
    /// Получить голосование по ID (для внутреннего использования)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InternalVotingDto>> GetVotingById(Guid id)
    {
        try
        {
            var voting = await _votingService.GetVotingByIdAsync(id);

            if (voting == null)
            {
                return NotFound("Голосование не найдено");
            }

            var dto = new InternalVotingDto
            {
                Id = voting.Id,
                QuestionPut = voting.QuestionPut,
                ResponseOptions = voting.ResponseOptions
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении голосования {Id}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Принять голос от Telegram (для всех квартир пользователя)
    /// </summary>
    [HttpPost("{id}/vote-telegram")]
    public async Task<ActionResult> SubmitVoteFromTelegram(Guid id, [FromBody] TelegramVoteRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var message = await _votingService.SubmitVoteFromTelegramAsync(id, request.UserId, request.Response);
            return Ok(new { message });
        }
        catch (AlreadyVotedException ex)
        {
            // Возвращаем специальный статус для уже проголосовавших
            return StatusCode(409, new
            {
                message = ex.Message,
                previousResponse = ex.PreviousResponse,
                alreadyVoted = true
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при принятии голоса от Telegram");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Принять голос (для внутреннего использования из Telegram) - УСТАРЕВШИЙ
    /// </summary>
    [HttpPost("{id}/vote")]
    public async Task<ActionResult> SubmitVote(Guid id, [FromBody] VoteRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Здесь НЕ проверяем JWT, так как запрос приходит от TelegramBotService
            var message = await _votingService.SubmitVoteAsync(id, request, request.UserId);
            return Ok(new { message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при принятии голоса");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}