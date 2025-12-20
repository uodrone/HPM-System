using DTO;
using VotingService.Models;

namespace VotingService.Services;

public interface IVotingService
{
    Task<List<Voting>> GetAllVotingsAsync();
    Task<Voting> CreateVotingAsync(CreateVotingRequestDto request);
    Task<string> SubmitVoteAsync(Guid votingId, VoteRequestDto request, Guid userId);
    Task<VotingResultDto> GetVotingResultsAsync(Guid id);
    Task SetVotingDecisionAsync(Guid id, string decision);
    Task DeleteVotingAsync(Guid id);
    Task<VotingDetailDto?> GetVotingDetailByIdAsync(Guid id, Guid userId);
    Task<List<UserVotingDto>> GetMyVotingsAsync(Guid userId); // Переименовали
    Task<List<UserVotingDto>> GetMyActiveVotingsAsync(Guid userId); // Переименовали
    Task<List<UserVotingDto>> GetMyCompletedVotingsAsync(Guid userId); // Переименовали
    Task<List<UnresolvedVotingDto>> GetCompletedVotingsWithoutDecisionAsync();
}