using OrganizationsService.Domain.Entities;
using OrganizationsService.Domain.Repositories;

namespace OrganizationsService.Unittests.Fakes;

/// <summary>
/// Simple in-memory repository used to test the business logic without a database.
/// </summary>
internal sealed class InMemoryOrganizationRepository : IOrganizationRepository
{
    private int _nextId = 1;

    /// <summary>
    /// All stored organizations, exposed so tests can assert on the persisted state.
    /// </summary>
    public List<Organization> Items { get; } = [];

    /// <summary>
    /// The search value received by the last <see cref="ListAsync"/> call.
    /// </summary>
    public string? LastSearch { get; private set; }

    public InMemoryOrganizationRepository Seed(params Organization[] organizations)
    {
        foreach (var organization in organizations)
        {
            organization.Id = _nextId++;
            Items.Add(organization);
        }
        return this;
    }

    public Task<IReadOnlyList<Organization>> ListAsync(string? search, CancellationToken ct = default)
    {
        LastSearch = search;
        IEnumerable<Organization> query = Items;
        if (search is not null)
            query = query.Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || o.Tin.Contains(search));
        return Task.FromResult<IReadOnlyList<Organization>>(query.OrderBy(o => o.Name).ToList());
    }

    public Task<Organization?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(o => o.Id == id));

    public Task<bool> TinInUseAsync(string tin, int? excludeId = null, CancellationToken ct = default) =>
        Task.FromResult(Items.Any(o => o.Tin == tin && o.Id != excludeId));

    public Task AddAsync(Organization organization, CancellationToken ct = default)
    {
        organization.Id = _nextId++;
        Items.Add(organization);
        return Task.CompletedTask;
    }

    // The service mutates the stored instance directly, so there is nothing else to persist.
    public Task UpdateAsync(Organization organization, CancellationToken ct = default) => Task.CompletedTask;

    public Task DeleteAsync(Organization organization, CancellationToken ct = default)
    {
        Items.Remove(organization);
        return Task.CompletedTask;
    }
}
