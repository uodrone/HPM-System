using DTO;
using HPM_System.VotingService.CustomExceptions;
using VotingService.Models;
using VotingService.Repositories;
using VotingService.Validation;

namespace VotingService.Services;

public class VotingService : IVotingService
{
    private readonly IVotingRepository _repository;
    private readonly IApartmentServiceClient _apartmentServiceClient;
    private readonly IVotingEventPublisher _eventPublisher;
    private readonly ILogger<VotingService> _logger;
    private readonly VoteValidatorFactory _validatorFactory; // Добавили

    public VotingService(
        IVotingRepository repository,
        IApartmentServiceClient apartmentServiceClient,
        IVotingEventPublisher eventPublisher,
        VoteValidatorFactory validatorFactory, // Добавили
        ILogger<VotingService> logger)
    {
        _repository = repository;
        _apartmentServiceClient = apartmentServiceClient;
        _eventPublisher = eventPublisher;
        _validatorFactory = validatorFactory; // Добавили
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

        var participants = new List<(Guid UserId, long ApartmentId)>();

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
                    participants.Add((user.UserId, apartment.Id));
                }
            }
        }

        var createdVoting = await _repository.CreateVotingAsync(voting);

        // Публикуем событие для Telegram
        try
        {
            await _eventPublisher.PublishVotingCreatedAsync(
                createdVoting.Id,
                createdVoting.QuestionPut,
                createdVoting.ResponseOptions,
                createdVoting.EndTime,
                participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось опубликовать событие создания голосования {VotingId}", createdVoting.Id);
            // Не бросаем исключение, так как голосование уже создано
        }

        return createdVoting;
    }

    public async Task<string> SubmitVoteAsync(Guid votingId, VoteRequestDto request, Guid userId)
    {
        // Проверка, что userId совпадает
        if (request.UserId != userId)
            throw new UnauthorizedAccessException("Вы можете голосовать только от своего имени");

        // Валидация через цепочку валидаторов
        var context = new VoteContext
        {
            VotingId = votingId,
            UserId = userId,
            Response = request.Response,
            IsFromTelegram = false
        };

        // Используем единую цепочку валидаторов
        var validatorChain = _validatorFactory.CreateVotingValidatorChain();
        var validationResult = await validatorChain.ValidateAsync(context);

        if (!validationResult.IsValid)
        {
            throw validationResult.ErrorType switch
            {
                ValidationErrorType.NotFound => new KeyNotFoundException(validationResult.ErrorMessage),
                ValidationErrorType.Authorization => new UnauthorizedAccessException(validationResult.ErrorMessage),
                ValidationErrorType.InvalidInput => new ArgumentException(validationResult.ErrorMessage),
                ValidationErrorType.BusinessRule => new InvalidOperationException(validationResult.ErrorMessage),
                _ => new InvalidOperationException(validationResult.ErrorMessage)
            };
        }

        // Голосование за все квартиры пользователя
        var voting = context.Voting!;
        var userOwners = context.UserOwners!;

        _logger.LogInformation(
            "Веб-голосование начато. Пользователь {UserId} голосует в голосовании {VotingId} за вариант '{Response}'. Квартир: {Count}",
            userId, votingId, request.Response, userOwners.Count);

        // Группируем по домам для расчета веса
        var ownersByHouse = userOwners.GroupBy(o => o.HouseId);

        decimal totalWeight = 0;
        var apartmentDetails = new List<string>();

        foreach (var houseGroup in ownersByHouse)
        {
            var houseId = houseGroup.Key;

            // Считаем общую площадь дома
            var totalHouseArea = voting.OwnersList
                .Where(o => o.HouseId == houseId)
                .Sum(o => o.ApartmentArea);

            if (totalHouseArea == 0)
            {
                _logger.LogWarning("Общая площадь дома {HouseId} равна нулю", houseId);
                continue;
            }

            // Устанавливаем вес и ответ для каждой квартиры пользователя в этом доме
            foreach (var owner in houseGroup)
            {
                owner.VoteWeight = (owner.ApartmentArea * owner.Share) / totalHouseArea;
                owner.Response = request.Response;
                totalWeight += owner.VoteWeight;

                apartmentDetails.Add($"Кв. {owner.ApartmentId}: вес {Math.Round(owner.VoteWeight, 4)}");

                _logger.LogDebug(
                    "Квартира {ApartmentId}: площадь={Area}, доля={Share}, вес={Weight}",
                    owner.ApartmentId, owner.ApartmentArea, owner.Share, owner.VoteWeight);
            }
        }

        await _repository.SaveChangesAsync();
        await CheckAndSetVotingCompletedAsync(voting);

        _logger.LogInformation(
            "Веб-голосование завершено успешно. UserId={UserId}, VotingId={VotingId}, TotalWeight={Weight}",
            userId, votingId, totalWeight);

        // Формируем подробное сообщение
        if (userOwners.Count > 1)
        {
            return $"Голос принят с суммарным весом: {Math.Round(totalWeight, 4)}\n" +
                   $"Учтены все ваши квартиры ({userOwners.Count} шт.): {string.Join(", ", apartmentDetails)}";
        }
        else
        {
            return $"Голос принят с весом: {Math.Round(totalWeight, 4)}";
        }
    }

    public async Task<string> SubmitVoteFromTelegramAsync(
        Guid votingId,
        Guid userId,
        string response)
    {
        // Шаг нумер 1: Валидация через цепочку валидаторов
        // Создаем контекст для валидации
        var context = new VoteContext
        {
            VotingId = votingId,
            UserId = userId,
            Response = response,
            IsFromTelegram = true
        };

        // Получаем цепочку валидаторов из фабрики
        var validatorChain = _validatorFactory.CreateVotingValidatorChain();

        // Запускаем валидацию через цепочку
        var validationResult = await validatorChain.ValidateAsync(context);

        // Если валидация не прошла, выбрасываем соответствующее исключение
        if (!validationResult.IsValid)
        {
            throw validationResult.ErrorType switch
            {
                ValidationErrorType.NotFound => new KeyNotFoundException(validationResult.ErrorMessage),
                ValidationErrorType.Authorization => new UnauthorizedAccessException(validationResult.ErrorMessage),
                ValidationErrorType.InvalidInput => new ArgumentException(validationResult.ErrorMessage),
                ValidationErrorType.BusinessRule => new InvalidOperationException(validationResult.ErrorMessage),
                _ => new InvalidOperationException(validationResult.ErrorMessage)
            };
        }

        // Шаг нумер 2: собственно выполнение голосования
        // К этому моменту все проверки пройдены, context содержит:
        // - context.Voting - загруженное голосование
        // - context.UserOwners - список записей пользователя

        var voting = context.Voting!;
        var userOwners = context.UserOwners!;

        _logger.LogInformation(
            "Валидация пройдена. Пользователь {UserId} голосует в голосовании {VotingId} за вариант '{Response}'. Квартир: {Count}",
            userId, votingId, response, userOwners.Count);

        // Группируем по домам для расчета веса
        var ownersByHouse = userOwners.GroupBy(o => o.HouseId);

        decimal totalWeight = 0;

        foreach (var houseGroup in ownersByHouse)
        {
            var houseId = houseGroup.Key;

            // Считаем общую площадь дома
            var totalHouseArea = voting.OwnersList
                .Where(o => o.HouseId == houseId)
                .Sum(o => o.ApartmentArea);

            if (totalHouseArea == 0)
            {
                _logger.LogWarning("Общая площадь дома {HouseId} равна нулю", houseId);
                continue;
            }

            // Устанавливаем вес и ответ для каждой квартиры пользователя в этом доме
            foreach (var owner in houseGroup)
            {
                owner.VoteWeight = (owner.ApartmentArea * owner.Share) / totalHouseArea;
                owner.Response = response;
                totalWeight += owner.VoteWeight;

                _logger.LogDebug(
                    "Квартира {ApartmentId}: площадь={Area}, доля={Share}, вес={Weight}",
                    owner.ApartmentId, owner.ApartmentArea, owner.Share, owner.VoteWeight);
            }
        }

        // Сохраняем изменения
        await _repository.SaveChangesAsync();

        // Проверяем, не завершилось ли голосование
        await CheckAndSetVotingCompletedAsync(voting);

        _logger.LogInformation(
            "Голосование завершено успешно. UserId={UserId}, VotingId={VotingId}, TotalWeight={Weight}",
            userId, votingId, totalWeight);

        return $"Голос принят с суммарным весом: {Math.Round(totalWeight, 4)} (квартир: {userOwners.Count})";
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

            _logger.LogInformation(
                "Голосование {VotingId} автоматически завершено - все проголосовали",
                voting.Id);
        }
    }
}