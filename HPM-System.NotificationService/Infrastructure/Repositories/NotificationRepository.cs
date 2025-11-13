using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Domain.Entities;
using HPM_System.NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.NotificationService.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync(bool notReadOnly = false)
        {
            // Сортировка по дате (новые сверху), без привязки к пользователю
            IQueryable<Notification> query = _context.Notifications
                .Include(n => n.Recipients);

            if (notReadOnly)
            {
                // Фильтр: оставляем только уведомления, у которых есть хотя бы один непрочитанный получатель
                query = query.Where(n => n.Recipients.Any(r => r.ReadAt == null));
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .Include(n => n.Recipients)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            // Загружаем уведомления, отсортированные так:
            // 1. Непрочитанные данным пользователем — сверху
            // 2. Прочитанные — ниже
            // В пределах каждой группы — по дате (новые первыми)

            var notifications = await _context.Notifications
                .Include(n => n.Recipients)
                .Where(n => n.Recipients.Any(r => r.UserId == userId))
                .OrderByDescending(n => n.Recipients
                    .Where(r => r.UserId == userId)
                    .Min(r => r.ReadAt == null ? 1 : 0)) // непрочитанные = 1 → выше
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications;
        }

        public async Task<bool> MarkAsReadAsync(Guid recipientId)
        {
            var recipient = await _context.NotificationUsers.FirstOrDefaultAsync(r => r.Id == recipientId);
            if (recipient == null || recipient.ReadAt.HasValue)
                return false;

            recipient.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Include(n => n.Recipients)
                .Where(n => n.Recipients.Any(r => r.UserId == userId && r.ReadAt == null))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.NotificationUsers
                .CountAsync(nu => nu.UserId == userId && nu.ReadAt == null);
        }

        public async Task<bool> MarkAsReadByIdsAsync(Guid notificationId, Guid userId)
        {
            var recipient = await _context.NotificationUsers
                .FirstOrDefaultAsync(nu => nu.NotificationId == notificationId && nu.UserId == userId);

            if (recipient == null || recipient.ReadAt.HasValue)
                return false;

            recipient.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
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
    }
}