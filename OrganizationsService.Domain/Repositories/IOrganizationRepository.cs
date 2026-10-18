using OrganizationsService.Domain.Entities;

namespace OrganizationsService.Domain.Repositories
{
    /// <summary>
    /// Interface for the Organization repository, providing methods for managing organization data.
    /// </summary>
    public interface IOrganizationRepository
    {
        /// <summary>
        /// Lists organizations based on the provided search criteria.
        /// </summary>
        /// <param name="search">The search criteria to filter organizations.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A read-only list of organizations that match the search criteria.</returns>
        public Task<IReadOnlyList<Organization>> ListAsync(string? search, CancellationToken ct);
        /// <summary>
        /// Gets an organization by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the organization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The organization with the specified ID, or null if not found.</returns>
        public Task<Organization?> GetByIdAsync(int id, CancellationToken ct);
        /// <summary>
        /// Adds a new organization to the repository.
        /// </summary>
        /// <param name="organization">The organization to add.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AddAsync(Organization organization, CancellationToken ct);
        /// <summary>
        /// Updates an existing organization in the repository.
        /// </summary>
        /// <param name="organization">The organization to update.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task UpdateAsync(Organization organization, CancellationToken ct);
        /// <summary>
        /// Checks if an organization with the specified INN is already in use.
        /// </summary>
        /// <param name="inn">The INN to check.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the INN is in use; otherwise, false.</returns>
        public Task<bool> InnInUseAsync(string inn, CancellationToken ct);
        /// <summary>
        /// Deletes an organization from the repository.
        /// </summary>
        /// <param name="organization">The organization to delete.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task DeleteAsync(Organization organization, CancellationToken ct);
    }
}
