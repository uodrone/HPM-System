using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Domain.Entities;

namespace HPM_System.NotificationService.Infrastructure.Repositories
{
    public class InMemoryNotificationRepository : INotificationRepository
    {
        private readonly List<Notification> _notifies = new();

        public Task<Notification> AddAsync(Notification notification)
        {
            _notifies.Add(notification);
            return Task.FromResult(notification);
        }

        public Task<IEnumerable<Notification>> GetAllAsync(bool notReadOnly)
        {
            var result = notReadOnly ? _notifies.Where(x => x.Recipients.Any(y => y.ReadAt == null)) : _notifies;

            return Task.FromResult(result.AsEnumerable());
        }

        public Task<Notification?> GetByIdAsync(Guid id)
        {
            var notification = _notifies.FirstOrDefault(n => n.Id == id);
            return Task.FromResult(notification);
        }

        public Task<bool> MarkAsReadAsync(Guid id)
        {
            var recipient = _notifies.SelectMany(n => n.Recipients).FirstOrDefault(r => r.Id == id);

            if (recipient != null && recipient.ReadAt == null)
            {
                recipient.ReadAt = DateTime.UtcNow;
            }

            return Task.FromResult(true);
        }
    }
}
