using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrganizationsService.Infrastructure.Persistence;

namespace OrganizationsService.IntegrationsTests.Infrastructure;

/// <summary>
/// Base class for tests that work with the database directly (without HTTP).
/// Every test runs inside its own transaction that is rolled back after the test,
/// so nothing written by one test is ever visible to other tests or left in the database.
/// </summary>
/// <remarks>
/// All contexts created via <see cref="CreateDbContext"/> share one connection and one transaction,
/// so a second context in the same test sees the uncommitted changes of the first one.
/// Not suitable for HTTP tests: the application under <c>WebApplicationFactory</c> opens its own connections.
/// </remarks>
public abstract class TransactionalTestBase(PostgresFixture fixture) : IAsyncLifetime
{
    private NpgsqlConnection _connection = null!;
    private NpgsqlTransaction _transaction = null!;

    /// <summary>
    /// The shared PostgreSQL container fixture.
    /// </summary>
    protected PostgresFixture Fixture { get; } = fixture;

    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(Fixture.ConnectionString);
        await _connection.OpenAsync();
        _transaction = await _connection.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        // Roll back everything the test wrote, even if it failed midway.
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }

    /// <summary>
    /// Creates a context bound to the test transaction.
    /// EF Core does not own the connection, so disposing the context keeps the transaction alive.
    /// </summary>
    protected AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.UseTransaction(_transaction);
        return db;
    }
}
