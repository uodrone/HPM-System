using HPM_System.TelegramBotService.Data;
using HPM_System.TelegramBotService.Interfaces;
using HPM_System.TelegramBotService.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace HPM_System.TelegramBotService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Telegram Bot
            var botToken = builder.Configuration["Telegram:BotToken"]
                ?? throw new InvalidOperationException("Telegram:BotToken не задан");
            builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));

            // DbContext
            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IDbTelegramUserService, DbTelegramUserService>();

            // HTTP клиент для UserService
            var userServiceUrl = builder.Configuration["Services:UserService:BaseUrl"]
                ?? "http://hpm-system.userservice:8080";

            builder.Services.AddHttpClient<UserServiceClient>(client =>
            {
                client.BaseAddress = new Uri(userServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Фоновые службы
            builder.Services.AddHostedService<TelegramBotHostedService>();
            builder.Services.AddHostedService<RabbitMqConsumerService>();

            // Логирование
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            // Применение миграций
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    await context.Database.MigrateAsync();
                    app.Logger.LogInformation("База данных успешно мигрирована");
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Ошибка при миграции базы данных");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
    }
}