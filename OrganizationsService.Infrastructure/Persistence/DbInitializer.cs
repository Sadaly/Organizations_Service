using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrganizationsService.Domain.Entities;

namespace OrganizationsService.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider service, CancellationToken ct = default)
        {
            using var scope = service.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));

            await db.Database.MigrateAsync(ct);
            logger.LogInformation("Migration completed.");

            if (config.GetValue("Database:Seed", true) && !await db.Organizations.AnyAsync(ct))
            {
                logger.LogInformation("Seeding database...");
                await db.Organizations.AddRangeAsync(
                    new Organization
                    {
                        Name = "Boring company", Tin = "7701234567", Address = "Moscow, Moscowscaya 1",
                        Phone = "+7 495 123-45-67", Email = "info@romashka.ru",
                        RegisteredAt = new DateOnly(2015, 3, 12),
                        Description = "No description"
                    },
                    new Organization
                    {
                        Name = "Whatever CO", Tin = "7809876543", Address = "Krasnodar, Krasnaya 1",
                        Phone = "+7 812 765-43-21", Email = "office@vector.ru",
                        RegisteredAt = new DateOnly(2019, 7, 1),
                        Description = "No description also"
                    });
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Database seeding completed.");
            }
        }
    }
}
