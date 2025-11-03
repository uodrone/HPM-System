
using HPMFileStorageService.Data;
using HPMFileStorageService.Services;
using HPMFileStorageService.Settings;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

namespace HPMFileStorageService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Подключение PostgreSQL
            builder.Services.AddDbContext<ApplicationDBContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Регистрация настроек MinIO
            builder.Services.Configure<MinIOSettings>(builder.Configuration.GetSection("MinIO"));
            // Регистрация сервиса MinIO
            builder.Services.AddScoped<IMinIOService, MinIOService>();
            // Регистрация настроек максимального размера файла
            builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUpload"));
            // Увеличиваем лимиты для multipart-запросов (загрузка файлов)
            builder.Services.Configure<FormOptions>(options =>
            {
                // Устанавливаем лимит больше, чем MaxFileSizeMB
                options.MultipartBodyLengthLimit = 104_857_600; // 100 МБ
                options.ValueLengthLimit = 104_857_600;
            });

            // Для Kestrel
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 104_857_600; // 100 МБ
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

            var app = builder.Build();

            // Автоматическое применение миграций при запуске
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDBContext>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Применение миграций базы данных...");
                    context.Database.Migrate();
                    logger.LogInformation("Миграции успешно применены");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ошибка при применении миграций базы данных");

                    // В production среде можно не выбрасывать исключение,
                    // а в Development лучше остановить запуск
                    if (app.Environment.IsDevelopment())
                    {
                        throw;
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
