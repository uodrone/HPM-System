namespace VotingService.Validation.Validators;

/// <summary>
/// Валидатор проверяет, что выбранный вариант ответа существует в списке опций
/// </summary>
public class ResponseOptionValidator : BaseVoteValidator
{
    protected override Task<ValidationResult> ValidateCore(VoteContext context)
    {
        if (!context.Voting!.ResponseOptions.Contains(context.Response))
        {
            return Task.FromResult(ValidationResult.Fail(
                $"Ответ '{context.Response}' не является допустимым вариантом для этого голосования",
                ValidationErrorType.InvalidInput));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}