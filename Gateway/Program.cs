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

            // === 1. CORS (должен быть ПЕРВЫМ) ===
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:55670",  // HPM-System фронтенд
                        "http://localhost:55671",
                        "https://localhost:55671"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            // === 2. JWT Authentication ===
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

            // === 3. Authorization Policies ===
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("authenticated", policy =>
                    policy.RequireAuthenticatedUser());
            });

            // === 4. YARP Reverse Proxy ===
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

            // === 5. Controllers & Swagger ===
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // === Middleware Pipeline (ПОРЯДОК ВАЖЕН!) ===
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 1. CORS должен быть первым
            app.UseCors("AllowFrontend");

            // 2. Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // 3. Routing
            app.UseRouting();

            // 4. YARP Proxy
            app.MapReverseProxy();

            // 5. Controllers
            app.MapControllers();

            app.Run();
        }
    }
}