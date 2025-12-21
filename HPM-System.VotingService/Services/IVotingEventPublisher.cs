namespace VotingService.Services;

public interface IVotingEventPublisher
{
    Task PublishVotingCreatedAsync(Guid votingId, string questionPut, List<string> responseOptions, DateTime endTime, List<(Guid UserId, long ApartmentId)> participants);
}