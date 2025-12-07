using HPM_System.EventService.DataContext;
using HPM_System.EventService.Interfaces;
using HPM_System.EventService.Services;
using HPM_System.EventService.Services.HttpClients;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace HPM_System.EventService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // HTTP клиенты дл€ св€зи с другими микросервисами
            builder.Services.AddHttpClient<IApartmentServiceClient, ApartmentServiceClient>(client =>
            {
                var baseUrl = builder.Configuration["Services:ApartmentService:BaseUrl"]
                              ?? "http://hpm-system.apartmentservice:8080";
                client.BaseAddress = new Uri(baseUrl);
            });

            builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>(client =>
            {
                var baseUrl = builder.Configuration["Services:NotificationService:BaseUrl"]
                              ?? "http://hpm-system.notificationservice:8080";
                client.BaseAddress = new Uri(baseUrl);
            });

            // ќсновной EventService
            builder.Services.AddScoped<IEventService, EventServiceImpl>();

            // Controllers & OpenAPI
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            // CORS
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

            // јвтомиграции
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            // Middleware
            app.UseCors("AllowAll");

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication(); // об€зательно дл€ получени€ User.Claims!
            app.UseAuthorization();

            // Endpoints
            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.Run();
        }
    }
}