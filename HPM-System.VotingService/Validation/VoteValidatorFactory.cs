using VotingService.Repositories;
using VotingService.Validation.Validators;

namespace VotingService.Validation;

/// <summary>
/// Фабрика для создания цепочки валидаторов
/// </summary>
public class VoteValidatorFactory
{
    private readonly IVotingRepository _repository;

    public VoteValidatorFactory(IVotingRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Создать цепочку валидаторов для голосования
    /// Используется и для веб, и для Telegram (голосование за все квартиры пользователя)
    /// </summary>
    public IVoteValidator CreateVotingValidatorChain()
    {
        var existsValidator = new VotingExistsValidator(_repository);
        var notCompletedValidator = new VotingNotCompletedValidator();
        var responseValidator = new ResponseOptionValidator();
        var participantValidator = new UserIsParticipantValidator();
        var notVotedValidator = new UserNotVotedValidator();

        existsValidator
            .SetNext(notCompletedValidator)
            .SetNext(responseValidator)
            .SetNext(participantValidator)
            .SetNext(notVotedValidator);

        return existsValidator;
    }

    /// <summary>
    /// Создать цепочку валидаторов только для проверки существования
    /// (например, для просмотра результатов)
    /// </summary>
    public IVoteValidator CreateLightValidatorChain()
    {
        return new VotingExistsValidator(_repository);
    }
}