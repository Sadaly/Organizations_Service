using Microsoft.EntityFrameworkCore;
using OrganizationsService.Domain.Entities;
using OrganizationsService.Domain.Exceptions;
using OrganizationsService.Infrastructure.Persistence;
using OrganizationsService.IntegrationsTests.Infrastructure;

namespace OrganizationsService.IntegrationsTests;

/// <summary>
/// Integration tests for the organization repository using a real PostgreSQL database.
/// Covers ILIKE searches, unique indexes, and the date type.
/// Each test runs in its own transaction that is rolled back afterwards (see <see cref="TransactionalTestBase"/>).
/// </summary>
[Collection(PostgresCollection.Name)]
public class OrganizationRepositoryTests(PostgresFixture fx) : TransactionalTestBase(fx)
{
    private static string NewTin() =>
        Random.Shared.NextInt64(1_000_000_000, 9_999_999_999).ToString();

    private static Organization NewOrg(string name, string? tin = null) =>
        new()
        {
            Name = name,
            Tin = tin ?? NewTin(),
            Description = "Test organization description",
            RegisteredAt = new DateOnly(2020, 2, 29)
        };

    [Fact]
    public async Task Add_ThenGetById_RoundTripsAllFields()
    {
        await using var db = CreateDbContext();

        var org = NewOrg("Round-Trip Test");
        org.Description = "Organization used for round-trip testing";
        org.Address = "Kazan";
        org.Phone = "+78001234567";
        org.Email = "test@example.com";

        await new OrganizationRepository(db).AddAsync(org);

        await using var db2 = CreateDbContext();
        var loaded = await new OrganizationRepository(db2).GetByIdAsync(org.Id);

        Assert.NotNull(loaded);

        Assert.Equal(
            (org.Id, org.Name, org.Tin, org.Description, org.Address,
                org.Phone, org.Email, org.RegisteredAt),
            (loaded.Id, loaded.Name, loaded.Tin, loaded.Description, loaded.Address,
                loaded.Phone, loaded.Email, loaded.RegisteredAt));
    }

    [Fact]
    public async Task List_SearchIsCaseInsensitive()
    {
        var marker = $"Case{Guid.NewGuid():N}"[..14];

        await using var db = CreateDbContext();
        var repo = new OrganizationRepository(db);

        await repo.AddAsync(NewOrg($"Company {marker}"));

        var found = await repo.ListAsync(marker.ToUpperInvariant());

        Assert.Contains(found, o => o.Name == $"Company {marker}");
    }

    [Fact]
    public async Task List_SearchTreatsPercentLiterally()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];

        await using var db = CreateDbContext();
        var repo = new OrganizationRepository(db);

        await repo.AddAsync(NewOrg($"{marker} 100% reliability"));
        await repo.AddAsync(NewOrg($"{marker} 100 years"));

        var found = await repo.ListAsync($"{marker} 100%");

        Assert.Equal(
            $"{marker} 100% reliability",
            Assert.Single(found).Name);
    }

    [Fact]
    public async Task TinExists_RespectsExcludeId()
    {
        await using var db = CreateDbContext();
        var repo = new OrganizationRepository(db);
        var org = NewOrg("Excluded Organization");

        await repo.AddAsync(org);

        Assert.True(await repo.TinInUseAsync(org.Tin));
        Assert.False(await repo.TinInUseAsync(org.Tin, excludeId: org.Id));
    }

    [Fact]
    public async Task Add_DuplicateTin_UniqueIndexMapsToDomainException()
    {
        var tin = NewTin();

        await using (var db = CreateDbContext())
        {
            await new OrganizationRepository(db)
                .AddAsync(NewOrg("First Organization", tin));
        }

        await using var db2 = CreateDbContext();

        // Bypass service-level validation to verify the PostgreSQL unique constraint.
        await Assert.ThrowsAsync<DuplicateTinException>(
            () => new OrganizationRepository(db2)
                .AddAsync(NewOrg("Second Organization", tin)));
    }

    [Fact]
    public async Task Delete_RemovesRow()
    {
        await using var db = CreateDbContext();
        var repo = new OrganizationRepository(db);
        var org = NewOrg("Organization to Delete");

        await repo.AddAsync(org);
        await repo.DeleteAsync(org);

        await using var check = CreateDbContext();

        Assert.False(
            await check.Organizations.AnyAsync(o => o.Id == org.Id));
    }

    [Fact]
    public async Task Changes_AreNotVisibleOutsideTestTransaction()
    {
        await using var db = CreateDbContext();
        var org = NewOrg("Uncommitted Organization");
        await new OrganizationRepository(db).AddAsync(org);

        // A context from the fixture uses its own connection, outside the test transaction.
        await using var outside = Fixture.CreateDbContext();

        Assert.True(await db.Organizations.AnyAsync(o => o.Tin == org.Tin));
        Assert.False(await outside.Organizations.AnyAsync(o => o.Tin == org.Tin));
    }
}