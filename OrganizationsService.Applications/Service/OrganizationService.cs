using OrganizationsService.Applications.Abstraction;
using OrganizationsService.Applications.DTO;
using OrganizationsService.Domain.Entities;
using OrganizationsService.Domain.Exceptions;
using OrganizationsService.Domain.Repositories;

namespace OrganizationsService.Applications.Service;

public class OrganizationService(IOrganizationRepository repository) : IOrganizationService
{
    // Also I think It will also be good to add UnitOfWork pattern to handle transactions, but for now, we will keep it simple.

    public async Task<IReadOnlyList<OrganizationDto>> GetAllAsync(string? search, CancellationToken ct = default)
    {
        var items = await repository.ListAsync(string.IsNullOrWhiteSpace(search) ? null : search.Trim(), ct);
        return items.Select(OrganizationDto.FromEntity).ToList();
    }

    public async Task<OrganizationDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var org = await repository.GetByIdAsync(id, ct);
        return org is null ? null : OrganizationDto.FromEntity(org);
    }

    public async Task<int> CreateAsync(int id, OrganizationInput input, CancellationToken ct = default)
    {
        var tin = input.Tin.Trim();
        // Check if TIN is already in use (database constraint will also prevent duplicates, but we want to provide a better error message)
        if (await repository.TinInUseAsync(tin, ct))
            throw new DuplicateTinException($"Organization with TIN '{tin}' already exists.");
        
        var org = new Organization();
        // TODO: use AutoMapper to map input to entity
        Apply(org, input);
        await repository.AddAsync(org, ct);
        return org.Id;
    }

    public async Task UpdateAsync(int id, OrganizationInput input, CancellationToken ct = default)
    {
        var org = await repository.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException($"Organization with ID '{id}' not found.");
        var tin = input.Tin.Trim();
        // Check if TIN is already in use (database constraint will also prevent duplicates, but we want to provide a better error message)
        if (await repository.TinInUseAsync(tin, ct))
            throw new DuplicateTinException($"Organization with TIN '{tin}' already exists.");
        
        // TODO: use AutoMapper to map input to entity
        Apply(org, input);
        await repository.UpdateAsync(org, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var org = await repository.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException($"Organization with ID '{id}' not found.");
        await repository.DeleteAsync(org, ct);
    }

    private static void Apply(Organization org, OrganizationInput input)
    {
        org.Name = input.Name.Trim();
        org.Tin = input.Tin.Trim();
        org.Address = NullIfEmpty(input.Address);
        org.Phone = NullIfEmpty(input.Phone);
        org.Email = NullIfEmpty(input.Email);
        org.Description = NullIfEmpty(input.Description);
        org.RegisteredAt = input.RegisteredAt;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}