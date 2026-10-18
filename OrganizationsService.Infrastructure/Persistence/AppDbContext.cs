using Microsoft.EntityFrameworkCore;
using OrganizationsService.Domain.Entities;

namespace OrganizationsService.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Organization> Organizations => Set<Organization>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
