using HPM_System.UserService.Data;
using Microsoft.EntityFrameworkCore;

namespace HPM_System.UserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ��������� EF Core (���� ����� ������ � �� � �������� ����������)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // ��������� �������� ��� ������
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<AppDbContext>();
                // �������� ������ ������������ ��������
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogWarning("������������ ��������: {Migrations}", string.Join(", ", pendingMigrations));
                }
                else
                {
                    logger.LogInformation("��� �������� ��� ���������.");
                }

                try
                {
                    logger.LogInformation($"���������� ��������...");
                    context.Database.Migrate();
                    logger.LogInformation("HPM Users �������� �������");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying HPM-System Users migrations: {ex.Message}");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // ������ middleware: ������ � ���� �������!

            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
