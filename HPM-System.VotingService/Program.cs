using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VotingService.Data;
using VotingService.Repositories;
using VotingService.Services;

namespace VotingService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Регистрация HTTP-клиента для ApartmentService
            var apartmentServiceUrl = builder.Configuration["Services:ApartmentService:BaseUrl"]
                          ?? throw new InvalidOperationException("Services:ApartmentService:BaseUrl не задан");

            builder.Services.AddHttpClient<IApartmentServiceClient, ApartmentServiceHttpClient>(client =>
            {
                client.BaseAddress = new Uri(apartmentServiceUrl);
            });

            // Для локальной разработки можно использовать Mock
            // builder.Services.AddScoped<IApartmentServiceClient, MockApartmentServiceClient>();

            // Регистрация DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Регистрация Repository и Service
            builder.Services.AddScoped<IVotingRepository, VotingRepository>();
            builder.Services.AddScoped<IVotingService, Services.VotingService>();

            // JWT будет читаться из переменных окружения в env, переданных через docker-compose
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey не задан");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<VotingExpirationService>();

            var app = builder.Build();

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