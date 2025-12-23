namespace VotingService.Validation.Validators;

/// <summary>
/// Валидатор для проверки конкретной квартиры (используется в веб-интерфейсе)
/// Проверяет, что пользователь владеет указанной квартирой и не голосовал за неё
/// </summary>
public class SpecificApartmentValidator : BaseVoteValidator
{
    private readonly long _apartmentId;

    public SpecificApartmentValidator(long apartmentId)
    {
        _apartmentId = apartmentId;
    }

    protected override Task<ValidationResult> ValidateCore(VoteContext context)
    {
        // Проверяем, есть ли у пользователя указанная квартира
        var owner = context.UserOwners!.FirstOrDefault(o => o.ApartmentId == _apartmentId);

        if (owner == null)
        {
            return Task.FromResult(ValidationResult.Fail(
                $"Вы не являетесь владельцем квартиры #{_apartmentId}",
                ValidationErrorType.Authorization));
        }

        // Проверяем, не голосовал ли уже за эту квартиру
        if (!string.IsNullOrEmpty(owner.Response))
        {
            return Task.FromResult(ValidationResult.Fail(
                $"Вы уже проголосовали за квартиру #{_apartmentId}",
                ValidationErrorType.BusinessRule));
        }

        // Сохраняем информацию о конкретной квартире в контексте
        context.SpecificApartmentId = _apartmentId;

        return Task.FromResult(ValidationResult.Success());
    }
}