using HPM_System.EventService.Models;

namespace HPM_System.EventService.Interfaces
{
    public interface IEventRepository
    {
        Task<Event> AddAsync(Event ev, CancellationToken ct = default);
        Task<Event?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<List<Event>> GetByIdsAsync(List<long> ids, CancellationToken ct = default);
        Task<int> GetSubscribedCountAsync(long eventId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
