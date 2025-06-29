using Duende.IdentityServer.Models;
using Duende.IdentityServer.AspNetIdentity;
using HPM_System.IdentityServer.Data;
using HPM_System.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Регистрируем логгер
            builder.Services.AddLogging(configure => configure.AddConsole());

            // Добавляем DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Добавляем Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Добавляем IdentityServer
            builder.Services.AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryClients(IdentityConfiguration.Clients)
                .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
                .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
                .AddAspNetIdentity<ApplicationUser>()
                .AddDeveloperSigningCredential(); // Только для разработки

            // Настройка параметров Identity
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false; // Можно отключить спецсимволы
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            });

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

            builder.Services.AddAuthentication()
            .AddCookie("Cookies", options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = "/api/account/login";
            });

            // MVC / API / Controllers
            builder.Services.AddControllers();

            // OpenAPI/Swagger
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Применяем миграции при старте
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    // Применяем миграции Identity
                    var context = services.GetRequiredService<AppDbContext>();

                    // Проверяем состояние миграций
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");
                        context.Database.Migrate();
                        logger.LogInformation("Identity migrations applied successfully.");
                    }
                    else
                    {
                        logger.LogInformation("Identity database is up to date.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying Identity migrations.");
                    throw;
                }
            }

            // Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Важные middleware: именно в этом порядке!

            app.UseCors("AllowAll"); // Перед UseIdentityServer()

            app.UseRouting();       // Нужен для IdentityServer

            app.UseIdentityServer(); // ТОЛЬКО ТАК!

            app.UseAuthentication(); // После UseIdentityServer()

            app.UseAuthorization();   // После UseAuthentication()

            app.MapControllers();

            app.Run();
        }
    }
}