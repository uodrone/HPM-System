using VotingService.Repositories;

namespace VotingService.Validation.Validators;

/// <summary>
/// Валидатор проверяет, существует ли голосование
/// Загружает объект Voting в контекст для последующих валидаторов
/// </summary>
public class VotingExistsValidator : BaseVoteValidator
{
    private readonly IVotingRepository _repository;

    public VotingExistsValidator(IVotingRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<ValidationResult> ValidateCore(VoteContext context)
    {
        // Загружаем голосование из БД
        context.Voting = await _repository.GetVotingByIdAsync(context.VotingId);

        if (context.Voting == null)
        {
            return ValidationResult.Fail(
                "Голосование не найдено",
                ValidationErrorType.NotFound);
        }

        return ValidationResult.Success();
    }
}