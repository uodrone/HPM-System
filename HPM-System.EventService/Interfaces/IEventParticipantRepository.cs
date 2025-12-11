using HPM_System.EventService.Models;


namespace HPM_System.EventService.Interfaces
{
    public interface IEventParticipantRepository
    {
        Task<EventParticipant?> GetAsync(long eventId, Guid userId, CancellationToken ct = default);
        Task<List<long>> GetEventIdsForUserAsync(Guid userId, CancellationToken ct = default);
        Task<List<Guid>> GetSubscribedUserIdsAsync(long eventId, CancellationToken ct = default);
        Task<bool> IsUserSubscribedAsync(long eventId, Guid userId, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<EventParticipant> participants, CancellationToken ct = default);
        Task<List<EventParticipant>> GetParticipantsForReminderAsync(
            DateTime from,
            DateTime to,
            bool is24h,
            CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
