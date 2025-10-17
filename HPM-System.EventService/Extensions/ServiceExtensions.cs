using HPM_System.EventService.Repositories;

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
            services.AddScoped<IImageModelRepository, ImageModelRepository>();
            services.AddScoped<IEventModelRepository, EventModelRepository>();
        }
    }
}
