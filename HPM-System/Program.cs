using HPM_System.Data; // ������ �� ��� ������������ ���, ���� �����
using HPM_System.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HPM_System
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // 1. ������ �������� IdentityServer
            var identityOptions = builder.Configuration.GetSection("IdentityServer").Get<IdentityServerSettings>();
            if (identityOptions == null)
            {
                throw new InvalidOperationException("�� ������� ��������� IdentityServer � appsettings.json");
            }

            // 2. ��������� EF Core (���� ����� ������ � �� � �������� ����������)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 3. ��������� MVC / �����������
            builder.Services.AddControllersWithViews();

            // 4. ��������� �������������� ����� JWT Bearer
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = identityOptions.Authority; // https://localhost:32797
                options.RequireHttpsMetadata = false; // ������ ��� ����������
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = true,
                    ValidIssuer = identityOptions.Authority, // ������ ��������� � Authority
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    RequireSignedTokens = true
                };
            });

            // 5. ��������� �����������
            builder.Services.AddAuthorization();

            // 6. ������ ����������
            var app = builder.Build();

            // ��������� �������� ��� ������
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    context.Database.Migrate();
                    Console.WriteLine("HPM-System migrations applied successfully.");
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

            app.UseAuthentication(); // ����� UseRouting()
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