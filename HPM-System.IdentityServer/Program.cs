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

            // Добавляем DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Добавляем Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Добавляем IdentityServer
            builder.Services.AddIdentityServer()
                .AddInMemoryClients(IdentityConfiguration.Clients)
                .AddInMemoryIdentityResources(IdentityConfiguration.IdentityResources)
                .AddInMemoryApiScopes(IdentityConfiguration.ApiScopes)
                .AddAspNetIdentity<ApplicationUser>()
                .AddDeveloperSigningCredential(); // Только для разработки

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
