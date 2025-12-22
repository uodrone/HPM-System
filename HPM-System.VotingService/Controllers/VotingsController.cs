using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VotingService.Extensions;
using VotingService.Models;
using VotingService.Services;

namespace VotingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VotingsController : ControllerBase
{
    private readonly IVotingService _votingService;
    private readonly ILogger<VotingsController> _logger;

    public VotingsController(IVotingService votingService, ILogger<VotingsController> logger)
    {
        _votingService = votingService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все голосования (для админа)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Voting>>> GetVotings()
    {
        try
        {
            var votings = await _votingService.GetAllVotingsAsync();
            return Ok(votings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка голосований");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Получить детальную информацию о голосовании для текущего пользователя
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VotingDetailDto>> GetVotingById(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var voting = await _votingService.GetVotingDetailByIdAsync(id, userId);

            if (voting == null)
            {
                return NotFound("Голосование не найдено");
            }

            return Ok(voting);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Попытка доступа без авторизации");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении голосования {Id}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Создать новое голосование
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Voting>> CreateVoting(CreateVotingRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var voting = await _votingService.CreateVotingAsync(request);
            return CreatedAtAction(nameof(GetVotingById), new { id = voting.Id }, voting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании голосования");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Проголосовать (userId берется из JWT)
    /// </summary>
    [HttpPost("{id}/vote")]
    public async Task<ActionResult> SubmitVote(Guid id, VoteRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.GetUserId();
            var message = await _votingService.SubmitVoteAsync(id, request, userId);
            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
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

    /// <summary>
    /// Получить результаты голосования
    /// </summary>
    [HttpGet("{id}/results")]
    public async Task<ActionResult<VotingResultDto>> GetVotingResults(Guid id)
    {
        try
        {
            var result = await _votingService.GetVotingResultsAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении результатов голосования");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Установить решение комиссии
    /// </summary>
    [HttpPost("{id}/decision")]
    public async Task<ActionResult> SetVotingDecision(Guid id, [FromBody] string decision)
    {
        try
        {
            await _votingService.SetVotingDecisionAsync(id, decision);
            return Ok("Решение вынесено");
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
            _logger.LogError(ex, "Ошибка при вынесении решения");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Удалить голосование
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVoting(Guid id)
    {
        try
        {
            await _votingService.DeleteVotingAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении голосования");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Получить все голосования текущего пользователя
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<UserVotingDto>>> GetMyVotings()
    {
        try
        {
            var userId = User.GetUserId();
            var votings = await _votingService.GetMyVotingsAsync(userId);
            return Ok(votings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении голосований пользователя");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Получить активные голосования текущего пользователя (где он ещё не проголосовал)
    /// </summary>
    [HttpGet("my/active")]
    public async Task<ActionResult<List<UserVotingDto>>> GetMyActiveVotings()
    {
        try
        {
            var userId = User.GetUserId();
            var votings = await _votingService.GetMyActiveVotingsAsync(userId);
            return Ok(votings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных голосований пользователя");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Получить завершённые голосования текущего пользователя (где он уже проголосовал)
    /// </summary>
    [HttpGet("my/completed")]
    public async Task<ActionResult<List<UserVotingDto>>> GetMyCompletedVotings()
    {
        try
        {
            var userId = User.GetUserId();
            var votings = await _votingService.GetMyCompletedVotingsAsync(userId);
            return Ok(votings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении завершенных голосований пользователя");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    /// <summary>
    /// Получить завершённые голосования без решения комиссии
    /// </summary>
    [HttpGet("completed-without-decision")]
    public async Task<ActionResult<List<UnresolvedVotingDto>>> GetCompletedVotingsWithoutDecision()
    {
        try
        {
            var votings = await _votingService.GetCompletedVotingsWithoutDecisionAsync();
            return Ok(votings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении нерешенных голосований");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}