using Microsoft.Extensions.Configuration;
using Npgsql;
using OrganizationsService.Infrastructure.Persistence;

namespace OrganizationsService.Unittests;

/// <summary>
/// Unit tests for resolving the PostgreSQL connection string from configuration.
/// </summary>
public class ConnectionStringResolverTests
{
    private static IConfiguration Config(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void Resolve_UsesDefaultConnectionString()
    {
        var cs = ConnectionStringResolver.Resolve(Config(new() { ["ConnectionStrings:Default"] = "Host=x;Database=y" }));

        Assert.Equal("Host=x;Database=y", cs);
    }

    [Fact]
    public void Resolve_PrefersDatabaseUrl()
    {
        var cs = ConnectionStringResolver.Resolve(Config(new()
        {
            ["ConnectionStrings:Default"] = "Host=x;Database=y",
            ["DATABASE_URL"] = "postgres://u:p@db:5433/orgs",
        }));

        var b = new NpgsqlConnectionStringBuilder(cs);
        Assert.Equal(("db", 5433, "orgs", "u", "p"), (b.Host, b.Port, b.Database, b.Username, b.Password));
    }

    [Fact]
    public void FromUrl_WithoutPort_UsesDefaultPortAndDecodesPassword()
    {
        var b = new NpgsqlConnectionStringBuilder(
            ConnectionStringResolver.FromUrl("postgresql://user:p%40ss@db.example.com/orgs"));

        Assert.Equal(5432, b.Port);
        Assert.Equal("user", b.Username);
        Assert.Equal("p@ss", b.Password);
    }

    [Fact]
    public void Resolve_NothingConfigured_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => ConnectionStringResolver.Resolve(Config([])));
    }
}
