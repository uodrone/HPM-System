// HPM_System.EventService.Repositories/EventParticipantRepository.cs
using HPM_System.EventService.DataContext;
using HPM_System.EventService.Interfaces;
using HPM_System.EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.EventService.Repositories
{
    public class EventParticipantRepository : IEventParticipantRepository
    {
        private readonly AppDbContext _context;

        public EventParticipantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EventParticipant?> GetAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            return await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, ct);
        }

        public async Task<List<long>> GetEventIdsForUserAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.EventParticipants
                .Where(p => p.UserId == userId)
                .Select(p => p.EventId)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task<bool> IsUserParticipantAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            return await _context.EventParticipants
                .AnyAsync(ep => ep.EventId == eventId && ep.UserId == userId, ct);
        }

        public async Task<List<Guid>> GetSubscribedUserIdsAsync(long eventId, CancellationToken ct = default)
        {
            return await _context.EventParticipants
                .Where(p => p.EventId == eventId && p.IsSubscribed)
                .Select(p => p.UserId)
                .ToListAsync(ct);
        }

        public async Task<bool> IsUserSubscribedAsync(long eventId, Guid userId, CancellationToken ct = default)
        {
            return await _context.EventParticipants
                .AnyAsync(p => p.EventId == eventId && p.UserId == userId && p.IsSubscribed, ct);
        }

        public async Task AddRangeAsync(IEnumerable<EventParticipant> participants, CancellationToken ct = default)
        {
            await _context.EventParticipants.AddRangeAsync(participants, ct);
        }

        public async Task<List<EventParticipant>> GetParticipantsForReminderAsync(
            DateTime from,
            DateTime to,
            bool is24h,
            CancellationToken ct = default)
        {
            var query = _context.EventParticipants
                .Include(p => p.Event) // чтобы знать EventDateTime
                .Where(p => p.IsSubscribed
                            && p.Event.EventDateTime >= from
                            && p.Event.EventDateTime < to);

            query = is24h
                ? query.Where(p => !p.Reminder24hSent)
                : query.Where(p => !p.Reminder2hSent);

            return await query.ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}