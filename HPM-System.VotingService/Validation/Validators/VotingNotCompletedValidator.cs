namespace VotingService.Validation.Validators;

/// <summary>
/// Валидатор проверяет, что голосование еще не завершено
/// </summary>
public class VotingNotCompletedValidator : BaseVoteValidator
{
    protected override Task<ValidationResult> ValidateCore(VoteContext context)
    {
        // К этому моменту Voting уже загружен предыдущим валидатором
        if (context.Voting!.IsCompleted)
        {
            return Task.FromResult(ValidationResult.Fail(
                "Голосование уже завершено",
                ValidationErrorType.BusinessRule));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}