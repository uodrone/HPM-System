using Duende.IdentityServer.Models;
using Duende.IdentityServer.AspNetIdentity;
using HPM_System.IdentityServer.Data;
using HPM_System.IdentityServer.Models;
using HPM_System.IdentityServer.Services; // ��������� namespace ��� CustomProfileService
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

            // ������������ ������
            builder.Services.AddLogging(configure => configure.AddConsole());

            // ��������� DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ��������� Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // ��������� IdentityServer
            builder.Services.AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
                // �������� ��������� �����������
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddInMemoryClients(IdentityConfiguration.Clients)
                .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
                .AddInMemoryApiResources(IdentityConfiguration.ApiResources) // ��������� API Resources
                .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
                .AddAspNetIdentity<IdentityUser>()
                .AddProfileService<CustomProfileService>() // ������������ ��� ���������� ������
                .AddDeveloperSigningCredential(); // ������ ��� ����������

            // ��������� ���������� Identity
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false; // ����� ��������� �����������
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            });

            // ��������� CORS
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

            // ��������� �������� ��� ������
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    // ��������� �������� Identity
                    var context = services.GetRequiredService<AppDbContext>();

                    // ��������� ��������� ��������
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation($"���������� {pendingMigrations.Count()} ��������...");
                        context.Database.Migrate();
                        logger.LogInformation("Identity �������� �������");
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

            // ������ middleware: ������ � ���� �������!

            app.UseCors("AllowAll"); // ����� UseIdentityServer()

            app.UseRouting();       // ����� ��� IdentityServer

            app.UseIdentityServer(); // ������ ���!

            app.UseAuthentication(); // ����� UseIdentityServer()

            app.UseAuthorization();   // ����� UseAuthentication()

            app.MapControllers();

            app.Run();
        }
    }
}