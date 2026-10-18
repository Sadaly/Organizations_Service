using Microsoft.Extensions.DependencyInjection;
using OrganizationsService.Applications.Abstraction;
using OrganizationsService.Applications.Service;

namespace OrganizationsService.Applications
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IOrganizationService, OrganizationService>();
            return services;
        }
    }
}
