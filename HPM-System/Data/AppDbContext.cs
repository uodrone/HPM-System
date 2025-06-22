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

        public DbSet<PersonRole> PersonRoles { get; set; }
    }
}
