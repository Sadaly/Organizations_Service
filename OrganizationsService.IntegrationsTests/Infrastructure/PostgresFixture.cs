using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OrganizationsService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace OrganizationsService.IntegrationsTests.Infrastructure;

/// <summary>
/// One PostgreSQL container is created for all tests in the assembly. The container is started before the first test and stopped after the last test.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("organizations_test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Factory = new TestAppFactory(ConnectionString);
        // Force the creation of the HttpClient to ensure that the application is started and the database is initialized.
        Factory.CreateClient().Dispose();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    public AppDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(ConnectionString).Options);

    private sealed class TestAppFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                // Remove the existing DbContext registration and replace it with a new one that uses the PostgreSQL container connection string.
                foreach (var d in services.Where(d =>
                             d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                             d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>)).ToList())
                    services.Remove(d);

                services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
            });
        }
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
