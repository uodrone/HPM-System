namespace VotingService.Validation;

// Интерфейс валидатора в цепочке ответственности
public interface IVoteValidator
{
    // Установить следующий валидатор в цепочке
    IVoteValidator SetNext(IVoteValidator next);

    Task<ValidationResult> ValidateAsync(VoteContext context);
}