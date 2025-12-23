namespace VotingService.Validation;

// Базовый класс для валидаторов, реализующий паттерн Chain of Responsibility
public abstract class BaseVoteValidator : IVoteValidator
{
    private IVoteValidator? _next;

    // Установить следующий валидатор в цепочке
    public IVoteValidator SetNext(IVoteValidator next)
    {
        _next = next;
        return next;
    }

    // Валидировать контекст. Сначала выполняется своя валидация, затем передается управление следующему валидатору
    public async Task<ValidationResult> ValidateAsync(VoteContext context)
    {
        // Выполняем свою валидацию
        var result = await ValidateCore(context);

        // Если валидация не прошла, возвращаем ошибку
        if (!result.IsValid)
            return result;

        // Если есть следующий валидатор, передаем ему управление
        if (_next != null)
            return await _next.ValidateAsync(context);

        // Если следующего нет и валидация прошла - успех
        return result;
    }

    // Метод, который должен быть реализован в конкретном валидаторе
    protected abstract Task<ValidationResult> ValidateCore(VoteContext context);
}