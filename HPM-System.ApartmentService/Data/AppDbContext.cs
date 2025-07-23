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

        // DbSet для связи Many-to-Many (если используется)
        public DbSet<ApartmentUser> ApartmentUsers { get; set; }
        public DbSet<User> Users { get; set; }

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

            // Настройка сущности User (если используется)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityByDefaultColumn();
            });

            // Настройка отношения Many-to-Many: Apartment <-> User через ApartmentUser
            modelBuilder.Entity<ApartmentUser>()
                .HasKey(au => new { au.ApartmentId, au.UserId });

            modelBuilder.Entity<ApartmentUser>()
                .HasOne(au => au.Apartment)
                .WithMany(a => a.Users)
                .HasForeignKey(au => au.ApartmentId);

            modelBuilder.Entity<ApartmentUser>()
                .HasOne(au => au.User)
                .WithMany(u => u.Apartments)
                .HasForeignKey(au => au.UserId);
        }
    }
}