using DTO;
using VotingService.Models;

namespace VotingService.Services;

public interface IVotingService
{
    Task<List<Voting>> GetAllVotingsAsync();
    Task<Voting> CreateVotingAsync(CreateVotingRequestDto request);
    Task<string> SubmitVoteAsync(Guid votingId, VoteRequestDto request);
    Task<VotingResultDto> GetVotingResultsAsync(Guid id);
    Task SetVotingDecisionAsync(Guid id, string decision);
    Task DeleteVotingAsync(Guid id);
    Task<List<UserVotingDto>> GetUnvotedVotingsByUserAsync(Guid userId);
    Task<List<UserVotingDto>> GetVotedVotingsByUserAsync(Guid userId);
    Task<List<UnresolvedVotingDto>> GetCompletedVotingsWithoutDecisionAsync();
}