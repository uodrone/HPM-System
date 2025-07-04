using HPM_System.Data; // Замени на своё пространство имён, если нужно
using HPM_System.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace HPM_System
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // 1. Чтение настроек IdentityServer
            var identityOptions = builder.Configuration.GetSection("IdentityServer").Get<IdentityServerSettings>();
            if (identityOptions == null)
            {
                throw new InvalidOperationException("Не найдены настройки IdentityServer в appsettings.json");
            }

            // 2. Настройка EF Core (если нужен доступ к БД в основном приложении)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 3. Добавляем MVC / контроллеры
            builder.Services.AddControllersWithViews();

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

            // 4. Настройка аутентификации через JWT Bearer
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = identityOptions.Authority; // https://localhost:32797
                options.RequireHttpsMetadata = false; // только для разработки
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = true,
                    ValidIssuer = identityOptions.Authority, // должен совпадать с Authority
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    RequireSignedTokens = true
                };
            });

            // 5. Включение авторизации
            builder.Services.AddAuthorization();

            // 6. Сборка приложения
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
                    logger.LogInformation("HPM миграции успешны");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying HPM-System migrations: {ex.Message}");
                }
            }

            // 7. Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ВАЖНО: Добавляем CORS middleware ПОСЛЕ UseRouting(), но ПЕРЕД UseAuthentication()
            app.UseCors("AllowAll");

            app.UseAuthentication(); // После UseRouting() и UseCors()
            app.UseAuthorization();   // После UseAuthentication()

            // 8. Маршруты MVC + API
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllers(); // API-контроллеры
            });

            app.Run();
        }
    }
}