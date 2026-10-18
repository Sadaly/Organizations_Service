using OrganizationsService.Applications.DTO;

namespace OrganizationsService.Applications.Abstraction;

public interface IOrganizationService
{
    /// <summary>
    /// Gets all organizations with optional search filter.
    /// </summary>
    /// <param name="search">The search filter to apply.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of organization DTOs.</returns>
    Task<IReadOnlyList<OrganizationDto>> GetAllAsync(string? search, CancellationToken ct = default);
    /// <summary>
    /// Gets an organization by its ID.
    /// </summary>
    /// <param name="id">The ID of the organization.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The organization DTO if found; otherwise, null.</returns>
    Task<OrganizationDto?> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// Creates a new organization with the specified input data.
    /// </summary>
    /// <param name="input">The input data for the new organization.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The ID of the newly created organization.</returns>
    Task<int> CreateAsync(OrganizationInput input, CancellationToken ct = default);
    /// <summary>
    /// Updates an existing organization with the specified input data.
    /// </summary>
    /// <param name="id">The ID of the organization.</param>
    /// <param name="input">The input data for the organization.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateAsync(int id, OrganizationInput input, CancellationToken ct = default);
    /// <summary>
    /// Deletes an organization by its ID.
    /// </summary>
    /// <param name="id">The ID of the organization.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAsync(int id, CancellationToken ct = default);
}