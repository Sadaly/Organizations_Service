using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrganizationsService.Domain.Repositories;
using OrganizationsService.Infrastructure.Persistence;

namespace OrganizationsService.Infrastructure
{
    // Prefer use of extension methods to add services to the container instead of writing everything in the Program.cs file.
    // This keeps the Program.cs file clean and organized, and allows for better separation of concerns.
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the AppDbContext with the dependency injection container.
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(ConnectionStringResolver.Resolve(configuration)));
            // Register the OrganizationRepository with the dependency injection container.
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            return services;
        }
    }
}
