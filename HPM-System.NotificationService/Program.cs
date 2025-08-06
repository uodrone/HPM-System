
using HPM_System.NotificationService.Application.Handlers;
using HPM_System.NotificationService.Application.Interfaces;
using HPM_System.NotificationService.Application.Services;
using HPM_System.NotificationService.Infrastructure.RabbitMQ;
using HPM_System.NotificationService.Infrastructure.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HPM_System.NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.Load();

            var builder = WebApplication.CreateBuilder(args);                       

            //Подгружаем зависимости всякие
            builder.Services.AddScoped<INotificationAppService, NotificationAppService>();
            builder.Services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();
            builder.Services.AddScoped<IRabbitUserHandler, RabbitUserHandler>();

            //Подгружаем BackgroundService
            builder.Services.AddHostedService<RabbitUserConsumer>();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                    );
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();            

            var app = builder.Build();

            //Оставим настройку сваггера под дев. на будущее, пока что всем дадим сваггер
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
