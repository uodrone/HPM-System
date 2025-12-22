using VotingService.Models;

namespace VotingService.Repositories;

public interface IVotingRepository
{
    Task<List<Voting>> GetAllVotingsAsync();
    Task<Voting?> GetVotingByIdAsync(Guid id);
    Task<Voting> CreateVotingAsync(Voting voting);
    Task UpdateVotingAsync(Voting voting);
    Task DeleteVotingAsync(Voting voting);
    Task<Owner?> GetOwnerByUserAndApartmentAsync(Guid votingId, Guid userId, long apartmentId);
    Task<List<Voting>> GetUnvotedVotingsByUserAsync(Guid userId);
    Task<List<Voting>> GetVotedVotingsByUserAsync(Guid userId);
    Task<List<Voting>> GetCompletedVotingsWithoutDecisionAsync();
    Task<List<Voting>> GetExpiredVotingsAsync();
    Task SaveChangesAsync();
}