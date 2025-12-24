using HPM_System.TelegramBotService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HPM_System.TelegramBotService
{
    /// <summary>
    /// Фабрика для создания DbContext во время миграций
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var connectionString = "Host=localhost;Port=5433;Database=TelegramBotService;Username=postgres;Password=Morr92631043";

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}