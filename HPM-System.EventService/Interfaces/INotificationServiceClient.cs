using HPM_System.EventService.DTOs;

namespace HPM_System.EventService.Interfaces
{
    public interface INotificationServiceClient
    {
        Task CreateAsync(CreateEventNotificationRequest request, CancellationToken ct = default);
        Task CreateReminderAsync(CreateEventNotificationRequest request, CancellationToken ct = default);
    }
}
