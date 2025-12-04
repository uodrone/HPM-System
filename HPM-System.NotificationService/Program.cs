
using HPM_System.NotificationService.Application.Handlers;
using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Application.Services;
using HPM_System.NotificationService.Infrastructure.Persistence;
using HPM_System.NotificationService.Infrastructure.RabbitMQ;
using HPM_System.NotificationService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HPM_System.NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Подгружаем BackgroundService
            builder.Services.AddHostedService<RabbitMQConsumer>();
            builder.Services.AddSingleton<RabbitMQProducer>();

            //Подгружаем зависимости всякие
            builder.Services.AddScoped<INotificationAppService, NotificationAppService>();
            //комментирую за ненадобностью, но это для того, чтобы была сцылка на примитивы синхронизации
            //builder.Services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IRabbitMQHandler, RabbitUserHandler>();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                    );
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            var app = builder.Build();

            // Применяем миграции при старте
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<AppDbContext>();
                try
                {
                    logger.LogInformation($"Применение миграций...");
                    context.Database.Migrate();
                    logger.LogInformation("HPM Apartment миграции успешны");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying HPM-System Apartments migrations: {ex.Message}");
                }
            }

            //Оставим настройку сваггера под дев. на будущее, пока что всем дадим сваггер
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
