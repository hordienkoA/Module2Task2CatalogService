using Catalog.BLL.Services;

namespace Catalog.Service.Api.DiConfiguration
{
    public static class ServicesMethods
    {
        public static IServiceCollection AddServices(this IServiceCollection services) {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            return services;
        }
    }
}
