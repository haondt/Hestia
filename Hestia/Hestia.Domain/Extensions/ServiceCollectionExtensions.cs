using Hestia.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hestia.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHestiaDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IIngredientsService, IngredientsService>();
            services.AddScoped<IUnitConversionsService, UnitConversionsService>();
            services.AddScoped<IDbSeeder, DbSeeder>();
            services.AddScoped<IDevDbSeeder, DevDbSeeder>();

            return services;
        }
    }
}
