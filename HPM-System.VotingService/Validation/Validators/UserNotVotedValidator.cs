using HPM_System.VotingService.CustomExceptions;
using VotingService.Services;

namespace VotingService.Validation.Validators;

/// <summary>
/// Валидатор проверяет, что пользователь еще не голосовал
/// Только для Telegram (для веб используется SpecificApartmentValidator)
/// </summary>
public class UserNotVotedValidator : BaseVoteValidator
{
    protected override Task<ValidationResult> ValidateCore(VoteContext context)
    {
        // Для веб-интерфейса проверка конкретной квартиры делается в SpecificApartmentValidator
        if (!context.IsFromTelegram)
        {
            return Task.FromResult(ValidationResult.Success());
        }

        // Проверяем, есть ли хотя бы один Owner с уже заполненным Response
        var alreadyVotedOwners = context.UserOwners!
            .Where(o => !string.IsNullOrEmpty(o.Response))
            .ToList();

        if (alreadyVotedOwners.Any())
        {
            var previousResponse = alreadyVotedOwners.First().Response;

            // Выбрасываем специальное исключение для Telegram
            throw new AlreadyVotedException(
                $"Вы уже проголосовали. Ваш выбор: {previousResponse}",
                previousResponse);
        }

        return Task.FromResult(ValidationResult.Success());
    }
}