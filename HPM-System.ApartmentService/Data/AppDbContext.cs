using Microsoft.EntityFrameworkCore;
using HPM_System.ApartmentService.Models;
using System.Text.Json;

namespace HPM_System.ApartmentService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Apartment> Apartment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка сущности Apartment
            modelBuilder.Entity<Apartment>(entity =>
            {
                // Указываем первичный ключ
                entity.HasKey(u => u.Id);

                // Настройка автоинкремента для PostgreSQL
                entity.Property(u => u.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                // Настройка обязательных полей
                entity.Property(u => u.Number).IsRequired();
                entity.Property(u => u.NumbersOfRooms).IsRequired();
                entity.Property(u => u.ResidentialArea).IsRequired();
                entity.Property(u => u.TotalArea).IsRequired();
                entity.Property(u => u.Floor);

                // Настройка для List<int> - сохраняем как JSON
                entity.Property(u => u.UserId)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("jsonb"); // Используем jsonb для лучшей производительности в PostgreSQL

                // Можно также использовать более простой подход с массивом PostgreSQL:
                // entity.Property(u => u.UserId)
                //     .HasColumnType("integer[]");

                // Индекс для поиска по пользователям
                entity.HasIndex(u => u.UserId)
                    .HasMethod("gin"); // GIN индекс для jsonb в PostgreSQL
            });
        }
    }
}