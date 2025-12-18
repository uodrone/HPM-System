using DTO;
using VotingService.Models;
using VotingService.Repositories;

namespace VotingService.Services;

public class VotingService : IVotingService
{
    private readonly IVotingRepository _repository;
    private readonly IApartmentServiceClient _apartmentServiceClient;
    private readonly ILogger<VotingService> _logger;

    public VotingService(
        IVotingRepository repository,
        IApartmentServiceClient apartmentServiceClient,
        ILogger<VotingService> logger)
    {
        _repository = repository;
        _apartmentServiceClient = apartmentServiceClient;
        _logger = logger;
    }

    public async Task<List<Voting>> GetAllVotingsAsync()
    {
        return await _repository.GetAllVotingsAsync();
    }

    public async Task<Voting> CreateVotingAsync(CreateVotingRequestDto request)
    {
        var voting = new Voting
        {
            Id = Guid.NewGuid(),
            QuestionPut = request.QuestionPut,
            ResponseOptions = request.ResponseOptions,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(request.DurationInHours),
            IsCompleted = false
        };

        foreach (var houseId in request.HouseIds)
        {
            var apartments = await _apartmentServiceClient.GetApartmentsByHouseIdAsync(houseId);

            foreach (var apartment in apartments)
            {
                foreach (var user in apartment.Users)
                {
                    var owner = new Owner
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.UserId,
                        ApartmentId = apartment.Id,
                        HouseId = houseId,
                        ApartmentArea = apartment.TotalArea,
                        Share = user.Share,
                        Response = string.Empty
                    };
                    voting.OwnersList.Add(owner);
                }
            }
        }

        return await _repository.CreateVotingAsync(voting);
    }

    public async Task<string> SubmitVoteAsync(Guid votingId, VoteRequestDto request)
    {
        var voting = await _repository.GetVotingByIdAsync(votingId);

        if (voting == null)
            throw new KeyNotFoundException("Голосование не найдено");

        if (voting.IsCompleted)
            throw new InvalidOperationException("Голосование уже завершено");

        if (!voting.ResponseOptions.Contains(request.Response))
            throw new ArgumentException($"Ответ '{request.Response}' не является допустимым вариантом для этого голосования.");

        var owner = voting.OwnersList
            .FirstOrDefault(o => o.UserId == request.UserId && o.ApartmentId == request.ApartmentId);

        if (owner == null)
            throw new ArgumentException("Указанный пользователь не является владельцем в этой квартире");

        if (!string.IsNullOrEmpty(owner.Response))
            throw new InvalidOperationException("Пользователь уже проголосовал");

        var totalHouseArea = voting.OwnersList
            .Where(o => o.HouseId == owner.HouseId)
            .Sum(o => o.ApartmentArea);

        if (totalHouseArea == 0)
            throw new InvalidOperationException("Общая площадь дома не может быть нулевой");

        owner.VoteWeight = (owner.ApartmentArea * owner.Share) / totalHouseArea;
        owner.Response = request.Response;

        await _repository.SaveChangesAsync();

        await CheckAndSetVotingCompletedAsync(voting);

        return $"Голос принят с весом: {owner.VoteWeight}";
    }

    public async Task<VotingResultDto> GetVotingResultsAsync(Guid id)
    {
        var voting = await _repository.GetVotingByIdAsync(id);

        if (voting == null)
            throw new KeyNotFoundException("Голосование не найдено");

        if (!voting.IsCompleted)
            throw new InvalidOperationException("Голосование ещё активно, результаты недоступны");

        var votedOwners = voting.OwnersList
            .Where(o => !string.IsNullOrEmpty(o.Response))
            .ToList();

        var totalVotedWeight = votedOwners.Sum(o => o.VoteWeight);

        var responses = votedOwners
            .GroupBy(o => o.Response)
            .ToDictionary(
                g => g.Key,
                g => totalVotedWeight > 0
                    ? Math.Round((double)(g.Sum(o => o.VoteWeight) / totalVotedWeight) * 100, 2)
                    : 0.0
            );

        string decision = string.IsNullOrEmpty(voting.Decision)
            ? "Решение не опубликовано"
            : voting.Decision;

        return new VotingResultDto
        {
            QuestionPut = voting.QuestionPut,
            TotalVotedWeight = totalVotedWeight,
            Responses = responses,
            Decision = decision
        };
    }

    public async Task SetVotingDecisionAsync(Guid id, string decision)
    {
        var voting = await _repository.GetVotingByIdAsync(id);

        if (voting == null)
            throw new KeyNotFoundException("Голосование не найдено");

        if (!voting.IsCompleted)
            throw new InvalidOperationException("Голосование ещё активно, нельзя вынести решение");

        if (string.IsNullOrWhiteSpace(decision))
            throw new ArgumentException("Решение не может быть пустым");

        voting.Decision = decision;
        await _repository.UpdateVotingAsync(voting);
    }

    public async Task DeleteVotingAsync(Guid id)
    {
        var voting = await _repository.GetVotingByIdAsync(id);

        if (voting == null)
            throw new KeyNotFoundException("Голосование не найдено");

        await _repository.DeleteVotingAsync(voting);
    }

    /// <summary>
    /// Получить все голосования пользователя (активные и завершенные)
    /// </summary>
    public async Task<List<UserVotingDto>> GetVotingsByUserIdAsync(Guid userId)
    {
        // Получаем все голосования, где пользователь есть в списке владельцев
        var allVotings = await _repository.GetAllVotingsAsync();

        var userVotings = allVotings
            .Where(v => v.OwnersList.Any(o => o.UserId == userId))
            .Select(v =>
            {
                var owner = v.OwnersList.First(o => o.UserId == userId);
                return new UserVotingDto
                {
                    VotingId = v.Id,
                    QuestionPut = v.QuestionPut,
                    EndTime = v.EndTime,
                    IsCompleted = v.IsCompleted,
                    Response = string.IsNullOrEmpty(owner.Response) ? null : owner.Response
                };
            })
            .OrderByDescending(v => v.EndTime) // Сортируем по дате окончания (новые сверху)
            .ToList();

        return userVotings;
    }

    public async Task<List<UserVotingDto>> GetUnvotedVotingsByUserAsync(Guid userId)
    {
        var votings = await _repository.GetUnvotedVotingsByUserAsync(userId);

        return votings.Select(v => new UserVotingDto
        {
            VotingId = v.Id,
            QuestionPut = v.QuestionPut,
            EndTime = v.EndTime,
            IsCompleted = v.IsCompleted,
            Response = null
        }).ToList();
    }

    public async Task<List<UserVotingDto>> GetVotedVotingsByUserAsync(Guid userId)
    {
        var votings = await _repository.GetVotedVotingsByUserAsync(userId);

        return votings.Select(v => new UserVotingDto
        {
            VotingId = v.Id,
            QuestionPut = v.QuestionPut,
            EndTime = v.EndTime,
            IsCompleted = v.IsCompleted,
            Response = v.OwnersList.First(o => o.UserId == userId && !string.IsNullOrEmpty(o.Response)).Response
        }).ToList();
    }

    public async Task<List<UnresolvedVotingDto>> GetCompletedVotingsWithoutDecisionAsync()
    {
        var votings = await _repository.GetCompletedVotingsWithoutDecisionAsync();

        return votings.Select(v => new UnresolvedVotingDto
        {
            VotingId = v.Id,
            QuestionPut = v.QuestionPut,
            StartTime = v.StartTime,
            EndTime = v.EndTime,
            IsCompleted = v.IsCompleted
        }).ToList();
    }

    private async Task CheckAndSetVotingCompletedAsync(Voting voting)
    {
        bool allVoted = voting.OwnersList.All(o => !string.IsNullOrEmpty(o.Response));

        if (allVoted && !voting.IsCompleted)
        {
            voting.IsCompleted = true;
            await _repository.UpdateVotingAsync(voting);
        }
    }
}