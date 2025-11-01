using HPM_System.EventService.Repositories;
using HPM_System.EventService.Services.Interfaces;

namespace HPM_System.EventService.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.ConfigureRepository();
        }

        public static void ConfigureRepository(this IServiceCollection services)
        {
            services.AddScoped<IEventService, Services.InterfacesImplementation.EventService>();
            services.AddScoped<IImageModelRepository, ImageModelRepository>();
            services.AddScoped<IEventModelRepository, EventModelRepository>();
        }
    }
}
