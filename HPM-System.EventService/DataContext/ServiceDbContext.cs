using HPM_System.EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.EventService.DataContext
{
    public class ServiceDbContext : DbContext
    {
        public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<EventModel> Events { get; set; }
        public DbSet<ImageModel> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventModel>(entity =>
            {
                entity.HasKey(u => u.EventId);
            });

            modelBuilder.Entity<ImageModel>(entity =>
            {
                entity.HasKey(c => c.ImageId);
            });
        }
    }
}
