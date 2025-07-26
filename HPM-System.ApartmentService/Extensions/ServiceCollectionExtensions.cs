using HPM_System.ApartmentService.Services;

namespace HPM_System.ApartmentService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApartmentServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Регистрация HttpClient для UserService
            services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
            {
                var baseUrl = configuration["Services:UserService:BaseUrl"] ?? "https://localhost:55681";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<IUserServiceClient, UserServiceClient>();

            return services;
        }
    }
}