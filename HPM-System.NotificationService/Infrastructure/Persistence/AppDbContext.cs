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

            //Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Id)
                    .ValueGeneratedNever(); // так как мы генерируем Guid вручную

                entity.Property(n => n.Title)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(n => n.Message).IsRequired();

                entity.Property(n => n.ImageUrl);

                entity.Property(n => n.CreatedAt).IsRequired();

                entity.Property(n => n.CreatedBy).IsRequired();

                entity.Property(n => n.Type).IsRequired().HasConversion<int>(); // сохраняем enum как int

                entity.Property(n => n.IsReadable).IsRequired().HasDefaultValue(true);

                // Индекс по дате создания для сортировки
                entity.HasIndex(n => n.CreatedAt);
                // Индекс по заголовку для быстрого поиска
                entity.HasIndex(n => n.Title);
            });

            // NotificationUsers (связь многие-ко-многим с доп. полем ReadAt)
            modelBuilder.Entity<NotificationUsers>(entity =>
            {
                entity.HasKey(nu => nu.Id);
                entity.Property(nu => nu.Id).ValueGeneratedNever(); //id генерится вручную в сервисе

                // Но если генерить id через EF Core:
                // .ValueGeneratedOnAdd();

                entity.Property(nu => nu.NotificationId)
                    .IsRequired();

                entity.Property(nu => nu.UserId)
                    .IsRequired();

                entity.Property(nu => nu.ReadAt)
                    .IsRequired(false); // nullable

                // Связь Notification -> NotificationUsers (1 ко многим)
                entity.HasOne(nu => nu.Notification)
                    .WithMany(n => n.Recipients)
                    .HasForeignKey(nu => nu.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Индекс для быстрого поиска уведомлений по пользователю
                entity.HasIndex(nu => nu.UserId);

                // Опционально: составной индекс, если нужно предотвратить дубли
                entity.HasIndex(nu => new { nu.NotificationId, nu.UserId }).IsUnique();
            });
        }
    }
}