using HPM_System.EventService.DataContext;
using HPM_System.EventService.Interfaces;
using HPM_System.EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.EventService.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Event> AddAsync(Event ev, CancellationToken ct = default)
        {
            _context.Events.Add(ev);
            await _context.SaveChangesAsync(ct);
            return ev;
        }

        public async Task<Event?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            return await _context.Events.FindAsync(new object[] { id }, ct);
        }

        public async Task<List<Event>> GetByIdsAsync(List<long> ids, CancellationToken ct = default)
        {
            return await _context.Events
                .Where(e => ids.Contains(e.Id))
                .ToListAsync(ct);
        }

        public async Task<int> GetSubscribedCountAsync(long eventId, CancellationToken ct = default)
        {
            return await _context.EventParticipants
                .CountAsync(p => p.EventId == eventId && p.IsSubscribed, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}