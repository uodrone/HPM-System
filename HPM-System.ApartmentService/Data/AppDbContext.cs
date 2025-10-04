// HPM_System.ApartmentService/Data/AppDbContext.cs

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
        public DbSet<ApartmentUser> ApartmentUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<ApartmentUserStatus> ApartmentUserStatuses { get; set; }
        public DbSet<House> House { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка сущности House
            modelBuilder.Entity<House>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();

                entity.Property(h => h.City).IsRequired().HasMaxLength(200);
                entity.Property(h => h.Street).IsRequired().HasMaxLength(200);
                entity.Property(h => h.Number).IsRequired().HasMaxLength(50);
                entity.Property(h => h.Entrances).IsRequired();
                entity.Property(h => h.Floors).IsRequired();
                entity.Property(h => h.HasGas).IsRequired();
                entity.Property(h => h.HasElectricity).IsRequired();
                entity.Property(h => h.HasElevator).IsRequired();
                entity.Property(h => h.HeadId).IsRequired(false); // GUID может быть null
                entity.Property(h => h.PostIndex).HasMaxLength(20);
                entity.Property(h => h.ApartmentsArea).IsRequired(false);
                entity.Property(h => h.TotalArea).IsRequired(false);
                entity.Property(h => h.LandArea).IsRequired(false);
                entity.Property(h => h.IsApartmentBuilding).IsRequired();
                entity.Property(h => h.builtYear).IsRequired();

                // Индекс по адресу для быстрого поиска
                entity.HasIndex(h => new { h.City, h.Street, h.Number }).IsUnique();

                // Опционально: индекс по HeadId для быстрого поиска домов по старшему
                entity.HasIndex(h => h.HeadId);
            });

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

                entity.HasIndex(a => a.HouseId);

                // настраиваем связь без навигационного свойства в Apartment
                entity.HasOne<House>()
                    .WithMany(h => h.Apartments) // навигационное свойство в House
                    .HasForeignKey(a => a.HouseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка сущности User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
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
                    .HasPrecision(5, 4)
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

                entity.HasIndex(aus => new { aus.ApartmentId, aus.UserId, aus.StatusId }).IsUnique();
            });

            // Seed данных для статусов
            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Name = "Владелец" },
                new Status { Id = 2, Name = "Жилец" },
                new Status { Id = 3, Name = "Прописан" },
                new Status { Id = 4, Name = "Временно проживающий" }
            );

            // Опционально: seed для тестового дома
            // modelBuilder.Entity<House>().HasData(
            //     new House
            //     {
            //         Id = 1,
            //         City = "Ижевск",
            //         Street = "Ленина",
            //         Number = "10",
            //         Entrances = 4,
            //         Floors = 9,
            //         HasGas = true,
            //         HasElectricity = true,
            //         HasElevator = true,
            //         IsApartmentBuilding = true
            //     }
            // );
        }
    }
}