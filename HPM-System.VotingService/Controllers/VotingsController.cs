using DTO;
using Microsoft.AspNetCore.Mvc;
using VotingService.Models;
using VotingService.Services;

namespace VotingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VotingsController : ControllerBase
{
    private readonly IVotingService _votingService;
    private readonly ILogger<VotingsController> _logger;

    public VotingsController(IVotingService votingService, ILogger<VotingsController> logger)
    {
        _votingService = votingService;
        _logger = logger;
    }

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

    [HttpPost]
    public async Task<ActionResult<Voting>> CreateVoting(CreateVotingRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var voting = await _votingService.CreateVotingAsync(request);
            return CreatedAtAction(nameof(GetVotings), new { id = voting.Id }, voting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании голосования");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("{id}/vote")]
    public async Task<ActionResult> SubmitVote(Guid id, VoteRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var message = await _votingService.SubmitVoteAsync(id, request);
            return Ok(message);
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

    [HttpGet("user/{userId}/active")]
    public async Task<ActionResult<List<UserVotingDto>>> GetUnvotedVotingsByUser(Guid userId)
    {
        try
        {
            var votings = await _votingService.GetUnvotedVotingsByUserAsync(userId);
            return Ok(votings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных голосований пользователя");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("user/{userId}/completed")]
    public async Task<ActionResult<List<UserVotingDto>>> GetVotedVotingsByUser(Guid userId)
    {
        try
        {
            var votings = await _votingService.GetVotedVotingsByUserAsync(userId);
            return Ok(votings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении завершенных голосований пользователя");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

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