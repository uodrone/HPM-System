namespace VotingService.Validation.Validators;

/// <summary>
/// Валидатор проверяет, является ли пользователь участником голосования
/// Загружает список владельцев (owner) в контекст
/// </summary>
public class UserIsParticipantValidator : BaseVoteValidator
{
    protected override Task<ValidationResult> ValidateCore(VoteContext context)
    {
        // Получаем все записи пользователя в этом голосовании
        context.UserOwners = context.Voting!.OwnersList
            .Where(o => o.UserId == context.UserId)
            .ToList();

        if (!context.UserOwners.Any())
        {
            return Task.FromResult(ValidationResult.Fail(
                "Вы не являетесь участником этого голосования",
                ValidationErrorType.Authorization));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}