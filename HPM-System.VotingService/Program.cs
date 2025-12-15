
using Microsoft.EntityFrameworkCore;
using VotingService.Data;
using VotingService.Services;

namespace VotingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();



            // Регистрация HTTP-клиента для ApartmentService
            var apartmentServiceUrl = builder.Configuration["Services:ApartmentService:BaseUrl"]
                          ?? throw new InvalidOperationException("Services:ApartmentService:BaseUrl не задан");

            builder.Services.AddHttpClient<IApartmentServiceClient, ApartmentServiceHttpClient>(client =>
            {
                client.BaseAddress = new Uri(apartmentServiceUrl);
            });

            // Регистрация mock-клиента (В ДАЛЬНЕЙШЕМ УДАЛИТЬ!)
            builder.Services.AddScoped<IApartmentServiceClient, MockApartmentServiceClient>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<VotingExpirationService>();

            var app = builder.Build();

            // Свагер и автомиграции
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
