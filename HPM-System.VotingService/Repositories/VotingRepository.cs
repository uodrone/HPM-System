using Microsoft.EntityFrameworkCore;
using VotingService.Data;
using VotingService.Models;

namespace VotingService.Repositories;

public class VotingRepository : IVotingRepository
{
    private readonly ApplicationDbContext _context;

    public VotingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Voting>> GetAllVotingsAsync()
    {
        return await _context.Votings
            .Include(v => v.OwnersList)
            .ToListAsync();
    }

    public async Task<Voting?> GetVotingByIdAsync(Guid id)
    {
        return await _context.Votings
            .Include(v => v.OwnersList)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Voting> CreateVotingAsync(Voting voting)
    {
        _context.Votings.Add(voting);
        await _context.SaveChangesAsync();
        return voting;
    }

    public async Task UpdateVotingAsync(Voting voting)
    {
        _context.Votings.Update(voting);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteVotingAsync(Voting voting)
    {
        _context.Votings.Remove(voting);
        await _context.SaveChangesAsync();
    }

    public async Task<Owner?> GetOwnerByUserAndApartmentAsync(Guid votingId, Guid userId, long apartmentId)
    {
        return await _context.Owners
            .FirstOrDefaultAsync(o => o.VotingId == votingId
                                   && o.UserId == userId
                                   && o.ApartmentId == apartmentId);
    }

    public async Task<List<Voting>> GetUnvotedVotingsByUserAsync(Guid userId)
    {
        return await _context.Votings
            .Include(v => v.OwnersList)
            .Where(v => v.OwnersList.Any(o => o.UserId == userId && string.IsNullOrEmpty(o.Response)))
            .ToListAsync();
    }

    public async Task<List<Voting>> GetVotedVotingsByUserAsync(Guid userId)
    {
        return await _context.Votings
            .Include(v => v.OwnersList)
            .Where(v => v.OwnersList.Any(o => o.UserId == userId && !string.IsNullOrEmpty(o.Response)))
            .ToListAsync();
    }

    public async Task<List<Voting>> GetCompletedVotingsWithoutDecisionAsync()
    {
        return await _context.Votings
            .Where(v => v.IsCompleted && string.IsNullOrEmpty(v.Decision))
            .ToListAsync();
    }

    public async Task<List<Voting>> GetExpiredVotingsAsync()
    {
        return await _context.Votings
            .Where(v => !v.IsCompleted && v.EndTime < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}