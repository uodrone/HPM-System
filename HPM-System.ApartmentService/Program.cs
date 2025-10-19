using HPM_System.ApartmentService.Data;
using HPM_System.ApartmentService.Interfaces;
using HPM_System.ApartmentService.Repositories;
using HPM_System.ApartmentService.Services;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ApartmentService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Настройка EF Core (если нужен доступ к БД в основном приложении)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                .LogTo(Console.WriteLine, LogLevel.Information));

            // Настройка JSON для минимальных API
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.SerializerOptions.WriteIndented = true;
            });

            // Настройка JSON для контроллеров MVC - ЕДИНСТВЕННЫЙ правильный способ
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // Регистрация HttpClient для взаимодействия с UserService
            builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>();
            builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

            //Регистрация репозиториев
            builder.Services.AddScoped<IApartmentRepository, ApartmentRepository>();
            builder.Services.AddScoped<IStatusRepository, StatusRepository>();
            builder.Services.AddScoped<IHouseRepository, HouseRepository>();

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
