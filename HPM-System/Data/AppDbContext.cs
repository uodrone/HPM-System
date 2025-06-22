using HPM_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Community> Communities { get; set; }
        public DbSet<PersonRole> PersonRoles { get; set; }

        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
    }
}
