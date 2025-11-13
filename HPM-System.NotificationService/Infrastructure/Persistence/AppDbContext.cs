using Microsoft.EntityFrameworkCore;
using HPM_System.NotificationService.Domain.Entities;
using HPM_System.NotificationService.Domain.ValueObjects;

namespace HPM_System.NotificationService.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationUsers> NotificationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Id).ValueGeneratedNever();

                entity.Property(n => n.Title)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(n => n.Message).IsRequired();
                entity.Property(n => n.ImageUrl);
                entity.Property(n => n.CreatedAt).IsRequired();
                entity.Property(n => n.CreatedBy).IsRequired();
                entity.Property(n => n.Type).IsRequired().HasConversion<int>();
                entity.Property(n => n.IsReadable).IsRequired().HasDefaultValue(true);

                // Важнючие индексы для больших объемов
                entity.HasIndex(n => n.CreatedAt)
                    .HasDatabaseName("IX_Notifications_CreatedAt");

                entity.HasIndex(n => n.Title)
                    .HasDatabaseName("IX_Notifications_Title");

                // Индекс для быстрого поиска по создателю
                entity.HasIndex(n => n.CreatedBy)
                    .HasDatabaseName("IX_Notifications_CreatedBy");

                // Составной индекс для сортировки по типу и дате
                entity.HasIndex(n => new { n.Type, n.CreatedAt })
                    .HasDatabaseName("IX_Notifications_Type_CreatedAt");
            });

            // NotificationUsers
            modelBuilder.Entity<NotificationUsers>(entity =>
            {
                entity.HasKey(nu => nu.Id);
                entity.Property(nu => nu.Id).ValueGeneratedNever();

                entity.Property(nu => nu.NotificationId).IsRequired();
                entity.Property(nu => nu.UserId).IsRequired();
                entity.Property(nu => nu.ReadAt).IsRequired(false);

                // Связь с Notification
                entity.HasOne(nu => nu.Notification)
                    .WithMany(n => n.Recipients)
                    .HasForeignKey(nu => nu.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 🔥 КРИТИЧНЫЕ ИНДЕКСЫ для масштабирования:

                // 1. Быстрый поиск всех уведомлений пользователя
                entity.HasIndex(nu => nu.UserId)
                    .HasDatabaseName("IX_NotificationUsers_UserId");

                // 2. Быстрый поиск всех получателей уведомления
                entity.HasIndex(nu => nu.NotificationId)
                    .HasDatabaseName("IX_NotificationUsers_NotificationId");

                // 3. Уникальность: один пользователь = одна запись на уведомление
                entity.HasIndex(nu => new { nu.NotificationId, nu.UserId })
                    .IsUnique()
                    .HasDatabaseName("IX_NotificationUsers_NotificationId_UserId_Unique");

                // САМЫЙ ВАЖНЫЙ: быстрый поиск непрочитанных уведомлений пользователя
                entity.HasIndex(nu => new { nu.UserId, nu.ReadAt, nu.NotificationId })
                    .HasDatabaseName("IX_NotificationUsers_UserId_ReadAt_NotificationId")
                    .HasFilter("\"ReadAt\" IS NULL"); // Partial index только для непрочитанных

                // Быстрый подсчет непрочитанных уведомлений
                entity.HasIndex(nu => new { nu.UserId, nu.ReadAt })
                    .HasDatabaseName("IX_NotificationUsers_UserId_ReadAt");

                // Быстрый поиск по времени прочтения (для аналитики)
                entity.HasIndex(nu => nu.ReadAt)
                    .HasDatabaseName("IX_NotificationUsers_ReadAt")
                    .HasFilter("\"ReadAt\" IS NOT NULL"); // Только прочитанные
            });
        }
    }
}