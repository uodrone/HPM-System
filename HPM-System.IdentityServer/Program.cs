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

            // ������������ ������
            builder.Services.AddLogging(configure => configure.AddConsole());

            // ��������� DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ��������� Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // ��������� IdentityServer
            builder.Services.AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryClients(IdentityConfiguration.Clients)
                .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
                .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
                .AddAspNetIdentity<ApplicationUser>()
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

            // MVC / API / Controllers
            builder.Services.AddControllers();

            // OpenAPI/Swagger
            builder.Services.AddOpenApi();

            var app = builder.Build();

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