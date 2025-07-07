using HPM_System.UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.UserService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
    }
}
