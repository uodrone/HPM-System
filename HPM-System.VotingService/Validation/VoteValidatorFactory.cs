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
    /// Создать цепочку валидаторов для голосования через Telegram
    /// (голосует за все квартиры сразу)
    /// </summary>
    public IVoteValidator CreateTelegramValidatorChain()
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
    /// Создать цепочку валидаторов для голосования через веб
    /// (голосует за конкретную квартиру)
    /// </summary>
    public IVoteValidator CreateWebValidatorChain(long apartmentId)
    {
        var existsValidator = new VotingExistsValidator(_repository);
        var notCompletedValidator = new VotingNotCompletedValidator();
        var responseValidator = new ResponseOptionValidator();
        var participantValidator = new UserIsParticipantValidator();
        var apartmentValidator = new SpecificApartmentValidator(apartmentId);

        existsValidator
            .SetNext(notCompletedValidator)
            .SetNext(responseValidator)
            .SetNext(participantValidator)
            .SetNext(apartmentValidator); // Проверка конкретной квартиры

        return existsValidator;
    }
}