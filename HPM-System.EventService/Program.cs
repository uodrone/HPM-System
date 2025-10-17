
using HPM_System.EventService.DataContext;
using HPM_System.EventService.Extensions;
using HPM_System.EventService.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System;

namespace HPM_System.EventService
{
    public class Program
    {
        private static readonly string _connectionName = "DefaultConnection";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var a = builder.Configuration.GetConnectionString(_connectionName);

            builder.Services.AddDbContext<ServiceDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString(_connectionName)));

            // Add services to the container.

            builder.Services.ConfigureServices();

            builder.Services.AddControllers();
            
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
                
            }

            app.UseCors("AllowAll");

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
