using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy.Transforms;

namespace Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Определяем параметры CORS для фронта
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:55670",
                        "https://localhost:55671"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            // JWT Аутентификация
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is required");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("JWT Auth Failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("JWT Token validated for user: {User}",
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    }
                };
            });

            // Политики авторизации
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("authenticated", policy =>
                    policy.RequireAuthenticatedUser());
            });

            // YARP Reverse Proxy
            builder.Services.AddReverseProxy()
                            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
                            .AddTransforms(transformBuilder =>
                            {
                                // Передаём заголовок Authorization во ВСЕ маршруты
                                transformBuilder.AddRequestTransform(async context =>
                                {
                                    if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authValue))
                                    {
                                        context.ProxyRequest.Headers.Authorization =
                                            System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authValue.ToString());
                                    }
                                });
                            });

            // Контроллеры (на всякий) и Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Дальше порядок должен быть строго соблюдён, иначе ничерта не работает и можно голову сломать почему

            // CORS должен быть первым
            app.UseCors("AllowFrontend");

            // Аутентификация
            app.UseAuthentication();

            // Роутинг — ДО авторизации!
            app.UseRouting();

            // Авторизация — ПОСЛЕ роутинга!
            app.UseAuthorization();

            // YARP — ПОСЛЕ авторизации
            app.MapReverseProxy();

            // Controllers
            app.MapControllers();

            app.Run();
        }
    }
}