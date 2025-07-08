using Microsoft.EntityFrameworkCore;
using HPM_System.ApartmentService.Models;

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

            // Настройка сущности User
            modelBuilder.Entity<Apartment>(entity =>
            {
                // Указываем первичный ключ
                entity.HasKey(u => u.Id);

                // Настройка автоинкремента для PostgreSQL
                entity.Property(u => u.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                // Опционально: настройка других полей
                entity.Property(u => u.Number).IsRequired();
                entity.Property(u => u.NumbersOfRooms).IsRequired();
                entity.Property(u => u.ResidentialArea).IsRequired();
                entity.Property(u => u.TotalArea).IsRequired();
                entity.Property(u => u.Floor);
                entity.Property(u => u.UserId).IsRequired();

                entity.HasIndex(u => u.UserId);  // индекс на id пользователя    
            });
        }
    }
}