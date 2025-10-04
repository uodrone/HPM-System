using HPM_System.UserService.Data;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.UserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Настройка EF Core (если нужен доступ к БД в основном приложении)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Поддержка CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader();
                });
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Применяем миграции при старте
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<AppDbContext>();
                // Получаем список неприменённых миграций
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogWarning("Неприменённые миграции: {Migrations}", string.Join(", ", pendingMigrations));
                }
                else
                {
                    logger.LogInformation("Все миграции уже применены.");
                }

                try
                {
                    logger.LogInformation($"Применение миграций...");
                    context.Database.Migrate();
                    logger.LogInformation("HPM Users миграции успешны");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying HPM-System Users migrations: {ex.Message}");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Важные middleware: именно в этом порядке!

            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
