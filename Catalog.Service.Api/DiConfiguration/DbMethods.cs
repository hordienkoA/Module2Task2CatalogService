using Catalog.DAL;
using Catalog.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Service.Api.DiConfiguration
{
    public static class DbMethods
    {
        public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("CatalogSqlite")));

            services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("CatalogSqlite")));
            return services;
        }
    }
}
