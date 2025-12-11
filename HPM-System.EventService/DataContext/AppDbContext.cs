using HPM_System.EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.EventService.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Event ===
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Description)
                      .HasMaxLength(2000); // ограничение по смыслу

                entity.Property(e => e.ImageUrl)
                      .HasMaxLength(1000);

                entity.Property(e => e.Place)
                      .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("NOW()");

                // Индексы для фильтрации
                entity.HasIndex(e => e.EventDateTime);
                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.CreatedAt);

                // Также полезен индекс по времени события + флагу (для поиска "актуальных" событий)
                modelBuilder.Entity<Event>()
                    .HasIndex(e => e.EventDateTime)
                    .HasDatabaseName("IX_Event_EventDateTime");
            });

            // === EventParticipant ===
            modelBuilder.Entity<EventParticipant>(entity =>
            {
                // Composite primary key: (EventId, UserId)
                entity.HasKey(ep => new { ep.EventId, ep.UserId });

                // Внешний ключ на Event (каскадное удаление)
                entity.HasOne<Event>()
                      .WithMany()
                      .HasForeignKey(ep => ep.EventId)
                      .OnDelete(DeleteBehavior.Cascade); // если удалим событие — удалятся и участники

                // Индексы для быстрого поиска
                entity.HasIndex(ep => ep.UserId);
                entity.HasIndex(ep => ep.IsSubscribed);
                entity.HasIndex(ep => new { ep.EventId, ep.IsSubscribed });
                entity.HasIndex(ep => ep.InvitedAt);

                // Для фоновой задачи: найти подписчиков, которым нужно отправить 24h напоминание
                entity.HasIndex(ep => new { ep.EventId, ep.IsSubscribed, ep.Reminder24hSent })
                      .HasDatabaseName("IX_EventParticipant_24hReminder");

                // Аналогично для 2h
                entity.HasIndex(ep => new { ep.EventId, ep.IsSubscribed, ep.Reminder2hSent })
                      .HasDatabaseName("IX_EventParticipant_2hReminder");
            });
        }
    }
}
