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

    public async Task<Voting?> GetVotingByIdAsync(Guid id)
    {
        return await _repository.GetVotingByIdAsync(id);
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

    public async Task<string> SubmitVoteAsync(Guid votingId, VoteRequestDto request, Guid userId)
    {
        var voting = await _repository.GetVotingByIdAsync(votingId);

        if (voting == null)
            throw new KeyNotFoundException("Голосование не найдено");

        if (voting.IsCompleted)
            throw new InvalidOperationException("Голосование уже завершено");

        if (!voting.ResponseOptions.Contains(request.Response))
            throw new ArgumentException($"Ответ '{request.Response}' не является допустимым вариантом для этого голосования.");

        // Проверяем, что userId из JWT совпадает с userId в запросе
        if (request.UserId != userId)
            throw new UnauthorizedAccessException("Вы можете голосовать только от своего имени");

        var owner = voting.OwnersList
            .FirstOrDefault(o => o.UserId == userId && o.ApartmentId == request.ApartmentId);

        if (owner == null)
            throw new ArgumentException("Вы не являетесь владельцем в этой квартире");

        if (!string.IsNullOrEmpty(owner.Response))
            throw new InvalidOperationException("Вы уже проголосовали");

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
    /// Получить все голосования пользователя с расширенной информацией
    /// </summary>
    public async Task<List<UserVotingDto>> GetMyVotingsAsync(Guid userId)
    {
        var allVotings = await _repository.GetAllVotingsAsync();

        var userVotings = allVotings
            .Where(v => v.OwnersList.Any(o => o.UserId == userId))
            .Select(v =>
            {
                var owner = v.OwnersList.First(o => o.UserId == userId);
                var totalParticipants = v.OwnersList.Count;
                var votedCount = v.OwnersList.Count(o => !string.IsNullOrEmpty(o.Response));

                return new UserVotingDto
                {
                    VotingId = v.Id,
                    QuestionPut = v.QuestionPut,
                    EndTime = v.EndTime,
                    IsCompleted = v.IsCompleted,
                    Response = string.IsNullOrEmpty(owner.Response) ? null : owner.Response,
                    TotalParticipants = totalParticipants,
                    VotedCount = votedCount,
                    HasDecision = !string.IsNullOrEmpty(v.Decision),
                    HasVoted = !string.IsNullOrEmpty(owner.Response)
                };
            })
            .ToList();

        // Разделяем на активные и завершённые
        var activeVotings = userVotings
            .Where(v => !v.IsCompleted)
            .OrderBy(v => v.EndTime) // Сначала самые срочные
            .ToList();

        var completedVotings = userVotings
            .Where(v => v.IsCompleted)
            .OrderByDescending(v => v.EndTime) // Сначала самые свежие
            .ToList();

        // Объединяем: сначала активные, потом завершённые
        return activeVotings.Concat(completedVotings).ToList();
    }

    /// <summary>
    /// Получить детальную информацию о голосовании для конкретного пользователя
    /// </summary>
    public async Task<VotingDetailDto?> GetVotingDetailByIdAsync(Guid id, Guid userId)
    {
        var voting = await _repository.GetVotingByIdAsync(id);

        if (voting == null)
            return null;

        var totalParticipants = voting.OwnersList.Count;
        var votedCount = voting.OwnersList.Count(o => !string.IsNullOrEmpty(o.Response));

        // Ищем текущего пользователя среди участников
        var userOwner = voting.OwnersList.FirstOrDefault(o => o.UserId == userId);
        var isParticipant = userOwner != null;
        var hasVoted = isParticipant && !string.IsNullOrEmpty(userOwner.Response);

        return new VotingDetailDto
        {
            Id = voting.Id,
            QuestionPut = voting.QuestionPut,
            ResponseOptions = voting.ResponseOptions,
            StartTime = voting.StartTime,
            EndTime = voting.EndTime,
            IsCompleted = voting.IsCompleted,
            Decision = voting.Decision,
            TotalParticipants = totalParticipants,
            VotedCount = votedCount,
            HasDecision = !string.IsNullOrEmpty(voting.Decision),
            IsParticipant = isParticipant,
            HasVoted = hasVoted,
            UserResponse = hasVoted ? userOwner.Response : null,
            UserApartmentId = isParticipant ? userOwner.ApartmentId : null
        };
    }

    /// <summary>
    /// Получить активные голосования пользователя с расширенной информацией
    /// </summary>
    public async Task<List<UserVotingDto>> GetMyActiveVotingsAsync(Guid userId)
    {
        var votings = await _repository.GetUnvotedVotingsByUserAsync(userId);

        return votings.Select(v =>
        {
            var totalParticipants = v.OwnersList.Count;
            var votedCount = v.OwnersList.Count(o => !string.IsNullOrEmpty(o.Response));

            return new UserVotingDto
            {
                VotingId = v.Id,
                QuestionPut = v.QuestionPut,
                EndTime = v.EndTime,
                IsCompleted = v.IsCompleted,
                Response = null,
                TotalParticipants = totalParticipants,
                VotedCount = votedCount,
                HasDecision = !string.IsNullOrEmpty(v.Decision),
                HasVoted = false
            };
        })
        .OrderBy(v => v.EndTime)
        .ToList();
    }

    /// <summary>
    /// Получить завершенные голосования пользователя с расширенной информацией
    /// </summary>
    public async Task<List<UserVotingDto>> GetMyCompletedVotingsAsync(Guid userId)
    {
        var votings = await _repository.GetVotedVotingsByUserAsync(userId);

        return votings.Select(v =>
        {
            var owner = v.OwnersList.First(o => o.UserId == userId && !string.IsNullOrEmpty(o.Response));
            var totalParticipants = v.OwnersList.Count;
            var votedCount = v.OwnersList.Count(o => !string.IsNullOrEmpty(o.Response));

            return new UserVotingDto
            {
                VotingId = v.Id,
                QuestionPut = v.QuestionPut,
                EndTime = v.EndTime,
                IsCompleted = v.IsCompleted,
                Response = owner.Response,
                TotalParticipants = totalParticipants,
                VotedCount = votedCount,
                HasDecision = !string.IsNullOrEmpty(v.Decision),
                HasVoted = true
            };
        })
        .OrderByDescending(v => v.EndTime)
        .ToList();
    }

    /// <summary>
    /// Получить завершенные голосования без решения с расширенной информацией
    /// </summary>
    public async Task<List<UnresolvedVotingDto>> GetCompletedVotingsWithoutDecisionAsync()
    {
        var votings = await _repository.GetCompletedVotingsWithoutDecisionAsync();

        return votings.Select(v =>
        {
            var totalParticipants = v.OwnersList.Count;
            var votedCount = v.OwnersList.Count(o => !string.IsNullOrEmpty(o.Response));

            return new UnresolvedVotingDto
            {
                VotingId = v.Id,
                QuestionPut = v.QuestionPut,
                StartTime = v.StartTime,
                EndTime = v.EndTime,
                IsCompleted = v.IsCompleted,
                TotalParticipants = totalParticipants,
                VotedCount = votedCount
            };
        })
        .OrderBy(v => v.EndTime)
        .ToList();
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