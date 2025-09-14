using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using HPM_System.IdentityServer.Services.AccountService;
using HPM_System.IdentityServer.Data;
using HPM_System.IdentityServer.Models;
using HPM_System.IdentityServer.Services;
using HPM_System.IdentityServer.Services.ErrorHandlingService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HPM_System.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Явно настраиваем Kestrel для работы по HTTP и HTTPS
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8080); // HTTP для внутренних запросов между контейнерами
                options.ListenAnyIP(8081, listenOptions =>
                {
                    listenOptions.UseHttps(); // HTTPS для внешних запросов
                });
            });

            // Регистрируем бизнес-сервисы
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();

            // Добавляем MVC с Views
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
            builder.Services.AddEndpointsApiExplorer();

            // Регистрируем логгер
            builder.Services.AddLogging(configure => configure.AddConsole());

            // Добавляем DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Добавляем Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-very-long-secret-key-here-minimum-256-bits";
            var issuer = jwtSettings["Issuer"] ?? "HPM_System.IdentityServer";
            var audience = jwtSettings["Audience"] ?? "HPM_System.Clients";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            })
            .AddCookie("Cookies", options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
            });

            // Добавляем IdentityServer
            builder.Services.AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddInMemoryClients(IdentityConfiguration.Clients)
                .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
                .AddInMemoryApiResources(IdentityConfiguration.ApiResources)
                .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
                .AddAspNetIdentity<IdentityUser>()
                .AddProfileService<CustomProfileService>()
                .AddDeveloperSigningCredential();

            // Настройка параметров Identity
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
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

            // OpenAPI/Swagger
            builder.Services.AddOpenApi();

            // HTTP клиент для внешних вызовов (UserService)
            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();

            var app = builder.Build();

            // Применяем миграции при старте
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation($"Применение {pendingMigrations.Count()} миграций...");
                        context.Database.Migrate();
                        logger.LogInformation("Identity миграции успешны");
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
                app.UseDeveloperExceptionPage();
            }

            // Возвращаем HTTPS редирект, но только для внешних запросов
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseCors("AllowAll");
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            // Маршруты MVC
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // API маршруты
            app.MapControllers();

            // Добавляем логирование информации о том, на каких URL слушаем
            var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
            startupLogger.LogInformation("IdentityServer запущен и слушает на порту 8080");

            app.Run();
        }
    }
}