using Microsoft.EntityFrameworkCore;
using VotingService.Models;

namespace VotingService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Voting> Votings { get; set; } = default!;
    public DbSet<Owner> Owners { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настроика каскадного удаление для связи Voting -> Owner
        modelBuilder.Entity<Owner>()
            .HasOne(o => o.Voting)
            .WithMany(v => v.OwnersList)
            .HasForeignKey(o => o.VotingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}