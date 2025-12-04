using HPM_System.EventService.Models;

namespace HPM_System.EventService.Repositories
{
    public interface IEventModelRepository : IRepository<EventModel>
    {
        Task<IEnumerable<EventModel>> GetAllUserEventsAsync(Guid userId, CancellationToken ct);
    }
}
