using HPM_System.EventService.DataContext;
using HPM_System.EventService.Interfaces;
using HPM_System.EventService.Repositories;
using HPM_System.EventService.Services;
using HPM_System.EventService.Services.HttpClients;
using HPM_System.EventService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace HPM_System.EventService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // === DbContext ===
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // === Репозитории ===
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IEventParticipantRepository, EventParticipantRepository>();

            // === JWT-аутентификация (только если секрет задан) ===
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (!string.IsNullOrEmpty(secretKey))
            {
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = !string.IsNullOrEmpty(issuer),
                            ValidateAudience = !string.IsNullOrEmpty(audience),
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = issuer,
                            ValidAudience = audience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                            ClockSkew = TimeSpan.FromMinutes(5)
                        };

                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILogger<Program>>();
                                logger.LogWarning("JWT Auth Failed: {Message}", context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILogger<Program>>();
                                logger.LogInformation("JWT Token validated for user: {User}",
                                    context.Principal?.Identity?.Name ?? "Unknown");
                                return Task.CompletedTask;
                            }
                        };
                    });
            }

            // === HTTP-клиенты ===
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

            // === Сервисы ===
            builder.Services.AddScoped<IEventService, EventServiceImpl>();
            builder.Services.AddHostedService<ReminderBackgroundService>();

            // === ASP.NET Core ===
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // === Автомиграции (только в Development) ===
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            // === Middleware ===
            app.UseCors("AllowAll");

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Включаем аутентификацию ТОЛЬКО если JWT был зарегистрирован
            if (!string.IsNullOrEmpty(secretKey))
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

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