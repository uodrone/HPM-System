using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Domain.Entities;

namespace HPM_System.NotificationService.Infrastructure.Repositories
{
    public class InMemoryNotificationRepository : INotificationRepository
    {
        private readonly object _lock = new();
        private readonly List<Notification> _notifies = new();

        public Task<Notification> AddAsync(Notification notification)
        {
            lock (_lock)
            {
                _notifies.Add(notification);
                return Task.FromResult(notification);
            }
        }

        public Task<IEnumerable<Notification>> GetAllAsync(bool notReadOnly = false)
        {
            lock (_lock)
            {
                var result = notReadOnly
                    ? _notifies.Where(x => x.Recipients.Any(y => y.ReadAt == null))
                    : _notifies;

                return Task.FromResult(result.ToList().AsEnumerable());
            }
        }

        public Task<Notification?> GetByIdAsync(Guid id)
        {
            lock (_lock)
            {
                var notification = _notifies.FirstOrDefault(n => n.Id == id);
                return Task.FromResult(notification);
            }
        }

        public Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            lock (_lock)
            {
                var userNotifications = _notifies
                    .Where(notification => notification.Recipients.Any(recipient => recipient.UserId == userId))
                    .ToList();

                return Task.FromResult(userNotifications.AsEnumerable());
            }
        }

        public Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            lock (_lock)
            {
                var userNotifications = _notifies
                    .Where(notification => notification.Recipients.Any(recipient => recipient.UserId == userId && recipient.ReadAt == null))
                    .ToList();

                return Task.FromResult(userNotifications.AsEnumerable());
            }
        }

        public Task<int> GetUnreadCountAsync(Guid userId)
        {
            lock (_lock)
            {
                var count = _notifies
                    .Sum(notification => notification.Recipients.Count(recipient => recipient.UserId == userId && recipient.ReadAt == null));

                return Task.FromResult(count);
            }
        }

        public Task<bool> MarkAsReadAsync(Guid recipientId)
        {
            lock (_lock)
            {
                var recipient = _notifies
                    .SelectMany(n => n.Recipients)
                    .FirstOrDefault(r => r.Id == recipientId);

                if (recipient != null && recipient.ReadAt == null)
                {
                    recipient.ReadAt = DateTime.UtcNow;
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        public Task<bool> MarkAsReadByIdsAsync(Guid notificationId, Guid userId)
        {
            lock (_lock)
            {
                var recipient = _notifies
                    .SelectMany(n => n.Recipients)
                    .FirstOrDefault(r => r.NotificationId == notificationId && r.UserId == userId);

                if (recipient != null && recipient.ReadAt == null)
                {
                    recipient.ReadAt = DateTime.UtcNow;
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        public Task<int> MarkAllAsReadAsync(Guid userId)
        {
            lock (_lock)
            {
                var recipients = _notifies
                    .SelectMany(n => n.Recipients)
                    .Where(r => r.UserId == userId && r.ReadAt == null)
                    .ToList();

                var now = DateTime.UtcNow;
                foreach (var recipient in recipients)
                {
                    recipient.ReadAt = now;
                }

                return Task.FromResult(recipients.Count);
            }
        }
    }
}