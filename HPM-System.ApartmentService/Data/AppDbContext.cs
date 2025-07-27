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

        // DbSet для работы с квартирами
        public DbSet<Apartment> Apartment { get; set; }

        // DbSet для связи Many-to-Many
        public DbSet<ApartmentUser> ApartmentUsers { get; set; }
        public DbSet<User> Users { get; set; }

        // DbSet для статусов
        public DbSet<Status> Statuses { get; set; }
        public DbSet<ApartmentUserStatus> ApartmentUserStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка сущности Apartment
            modelBuilder.Entity<Apartment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                entity.Property(a => a.Number).IsRequired();
                entity.Property(a => a.NumbersOfRooms).IsRequired();
                entity.Property(a => a.ResidentialArea).IsRequired();
                entity.Property(a => a.TotalArea).IsRequired();
                entity.Property(a => a.Floor).IsRequired(false);
                entity.Property(a => a.HouseId).IsRequired();
            });

            // Настройка сущности User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();
            });

            // Настройка сущности Status
            modelBuilder.Entity<Status>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(s => s.Name).IsUnique();
            });

            // Настройка отношения Many-to-Many: Apartment <-> User через ApartmentUser
            modelBuilder.Entity<ApartmentUser>(entity =>
            {
                entity.HasKey(au => new { au.ApartmentId, au.UserId });

                entity.Property(au => au.Share)
                    .HasPrecision(5, 4) // Точность для долей владения (например, 0.45)
                    .HasDefaultValue(0m);

                entity.HasOne(au => au.Apartment)
                    .WithMany(a => a.Users)
                    .HasForeignKey(au => au.ApartmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(au => au.User)
                    .WithMany(u => u.Apartments)
                    .HasForeignKey(au => au.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка сущности ApartmentUserStatus
            modelBuilder.Entity<ApartmentUserStatus>(entity =>
            {
                entity.HasKey(aus => aus.Id);
                entity.Property(aus => aus.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                entity.HasOne(aus => aus.ApartmentUser)
                    .WithMany(au => au.Statuses)
                    .HasForeignKey(aus => new { aus.ApartmentId, aus.UserId })
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(aus => aus.Status)
                    .WithMany()
                    .HasForeignKey(aus => aus.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Один пользователь не может иметь один и тот же статус дважды для одной квартиры
                entity.HasIndex(aus => new { aus.ApartmentId, aus.UserId, aus.StatusId }).IsUnique();
            });

            // Определяем тут возможные статусы пользователей квартиры
            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Name = "Владелец" },
                new Status { Id = 2, Name = "Жилец" },
                new Status { Id = 3, Name = "Прописан" },
                new Status { Id = 4, Name = "Временно проживающий" }
            );
        }
    }
}