using Catalog.Contracts.Interfaces;
using Catalog.DAL.Repositories;
using Catalog.DAL.UnitOfWork;

namespace Catalog.Service.Api.DiConfiguration
{
    public static class RepositoriesAndEowMethods
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            return services;
        }

        public static IServiceCollection AddUow(this IServiceCollection services) {
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            return services;
        }
    }
}
