using HPM_System.Data; // ������ �� ��� ������������ ���, ���� �����
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

            // 1. ��������� EF Core (���� ����� ������ � �� � �������� ����������)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. ��������� MVC / �����������
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

            // ��������� HttpClient
            builder.Services.AddHttpClient();

            // ����������� JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-very-long-secret-key-here-minimum-256-bits";
            var issuer = jwtSettings["Issuer"] ?? "HPM_System.IdentityServer";
            var audience = jwtSettings["Audience"] ?? "HPM_System.Clients";

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    // ����������� ��������� ������ �� cookie � ����������
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // ������� �������� �������� ����� �� ��������� Authorization
                            var token = context.Request.Headers.Authorization
                                .FirstOrDefault()?.Split(" ").LastOrDefault();

                            // ���� ������ ��� � ���������, �������� �������� �� cookie
                            if (string.IsNullOrEmpty(token))
                            {
                                token = context.Request.Cookies["auth_token"];
                            }

                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();
                            logger.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };
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

            // 3. ��������� �����������
            builder.Services.AddAuthorization();

            // ��������� ����������� � ������
            builder.Services.AddMemoryCache();

            // 4. ������ ����������
            var app = builder.Build();

            // ��������� �������� ��� ������
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<AppDbContext>();
                try
                {
                    logger.LogInformation($"���������� ��������...");
                    context.Database.Migrate();
                    logger.LogInformation("HPM �������� �������");
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

            // �����: ��������� CORS middleware ����� UseRouting(), �� ����� UseAuthentication()
            app.UseCors("AllowAll");

            app.UseAuthentication(); // ����� UseRouting() � UseCors()
            app.UseAuthorization();   // ����� UseAuthentication()

            // 8. �������� MVC + API
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllers(); // API-�����������
            });

            app.Run();
        }
    }
}