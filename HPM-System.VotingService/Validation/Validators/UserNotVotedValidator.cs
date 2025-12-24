using HPM_System.VotingService.CustomExceptions;
using VotingService.Services;

namespace VotingService.Validation.Validators;

// Валидатор проверяет, что пользователь еще не голосовал (ни за одну из своих квартир)
public class UserNotVotedValidator : BaseVoteValidator
{
    protected override Task<ValidationResult> ValidateCore(VoteContext context)
    {
        // Проверяем, есть ли хотя бы один Owner с уже заполненным Response
        var alreadyVotedOwners = context.UserOwners!
            .Where(o => !string.IsNullOrEmpty(o.Response))
            .ToList();

        if (alreadyVotedOwners.Any())
        {
            var previousResponse = alreadyVotedOwners.First().Response;

            // Если запрос из Telegram, выбрасываем специальное исключение
            if (context.IsFromTelegram)
            {
                throw new AlreadyVotedException(
                    $"Вы уже проголосовали. Ваш выбор: {previousResponse}",
                    previousResponse);
            }

            // Для веб-интерфейса тоже возвращаем ошибку
            return Task.FromResult(ValidationResult.Fail(
                $"Вы уже проголосовали. Ваш выбор: {previousResponse}",
                ValidationErrorType.BusinessRule));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}