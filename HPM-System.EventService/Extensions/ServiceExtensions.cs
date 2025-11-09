using HPM_System.EventService.Repositories;
using HPM_System.EventService.Services.Interfaces;
using HPM_System.EventService.Services.InterfacesImplementation;

namespace HPM_System.EventService.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.ConfigureHTTPClients(configuration);
            services.ConfigureRepository();
        }

        public static void ConfigureRepository(this IServiceCollection services)
        {
            services.AddScoped<IEventService, Services.InterfacesImplementation.EventService>();
            services.AddScoped<IImageModelRepository, ImageModelRepository>();
            services.AddScoped<IEventModelRepository, EventModelRepository>();
        }

        public static void ConfigureHTTPClients(this IServiceCollection services, ConfigurationManager configuration)
        {
            // Для взаимодействия с UserService
            services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, client) =>
            {
                var baseUrl = configuration["Services:UserService:BaseUrl"] ?? "http://hpm-system.userservice:8080";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Для взаимодействия с ApartmentService
            services.AddHttpClient<IApartmentServiceClient, ApartmentServiceClient>((sp, client) =>
            {
                var baseUrl = configuration["Services:ApartmentService:BaseUrl"] ?? "http://hpm-system.apartmentservice:8080";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }
    }
}
