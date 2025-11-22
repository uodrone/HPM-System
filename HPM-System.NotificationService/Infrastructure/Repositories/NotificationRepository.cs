using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Domain.Entities;
using HPM_System.NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace HPM_System.NotificationService.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private static readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(10, 10); // максимум 10 одновременных операций с БД

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                return notification;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<IEnumerable<Notification>> GetAllAsync(bool notReadOnly = false)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                IQueryable<Notification> query = _context.Notifications
                    .Include(n => n.Recipients);

                if (notReadOnly)
                {
                    query = query.Where(n => n.Recipients.Any(r => r.ReadAt == null));
                }

                return await query
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                return await _context.Notifications
                    .Include(n => n.Recipients)
                    .FirstOrDefaultAsync(n => n.Id == id);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var notifications = await _context.Notifications
                    .Include(n => n.Recipients)
                    .Where(n => n.Recipients.Any(r => r.UserId == userId))
                    .OrderByDescending(n => n.Recipients
                        .Where(r => r.UserId == userId)
                        .Min(r => r.ReadAt == null ? 1 : 0))
                    .ThenByDescending(n => n.CreatedAt)
                    .ToListAsync();

                return notifications;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid recipientId)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var recipient = await _context.NotificationUsers.FirstOrDefaultAsync(r => r.Id == recipientId);
                if (recipient == null || recipient.ReadAt.HasValue)
                    return false;

                recipient.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var notifications = await _context.Notifications
                    .Include(n => n.Recipients)
                    .Where(n => n.Recipients.Any(r => r.UserId == userId && r.ReadAt == null))
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                return notifications;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                return await _context.NotificationUsers
                    .CountAsync(nu => nu.UserId == userId && nu.ReadAt == null);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<bool> MarkAsReadByIdsAsync(Guid notificationId, Guid userId)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var recipient = await _context.NotificationUsers
                    .FirstOrDefaultAsync(nu => nu.NotificationId == notificationId && nu.UserId == userId);

                if (recipient == null || recipient.ReadAt.HasValue)
                    return false;

                recipient.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                var unreadRecipients = await _context.NotificationUsers
                    .Where(nu => nu.UserId == userId && nu.ReadAt == null)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                foreach (var recipient in unreadRecipients)
                {
                    recipient.ReadAt = now;
                }

                var count = unreadRecipients.Count;
                if (count > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return count;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }
    }
}