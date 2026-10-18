using OrganizationsService.Domain.Entities;

namespace OrganizationsService.Applications.DTO
{
    /// <summary>
    /// Represents an organization.
    /// </summary>
    /// <param name="Id">
    /// <example>1</example>
    /// The unique identifier of the organization.
    /// </param>
    /// <param name="Name">
    /// <example>Example LLC</example>
    /// The name of the organization.
    /// </param>
    /// <param name="Tin">
    /// <example>2310123456</example>
    /// The taxpayer identification number (TIN).
    /// </param>
    /// <param name="Description">
    /// <example>Supplier of office supplies</example>
    /// A description of the organization.
    /// </param>
    /// <param name="Address">
    /// <example>Krasnodar, Krasnaya 10</example>
    /// The registered address of the organization.
    /// </param>
    /// <param name="Phone">
    /// <example>+78001234567</example>
    /// The contact phone number of the organization.
    /// </param>
    /// <param name="Email">
    /// <example>info@example.com</example>
    /// The contact email address of the organization.
    /// </param>
    /// <param name="RegisteredAt">
    /// <example>2026-01-15</example>
    /// The date when the organization was registered.
    /// </param>
    public sealed record OrganizationDto
    (
        int Id,
        string Name,
        string Tin,
        string? Description,
        string? Address,
        string? Phone,
        string? Email,
        DateOnly RegisteredAt
    )
    {
        // Better to use AutoMapper for mapping, but for now we will do it manually.
        // Reason: I don't want to make project more complex than it is now, and I don't want to add more dependencies to the project,
        // when it has only one DTO and one entity. But if we will have more DTOs and entities, we will use AutoMapper.
        public static OrganizationDto FromEntity(Organization o) =>
            new(
                o.Id,
                o.Name,
                o.Tin,
                o.Description,
                o.Address,
                o.Phone,
                o.Email,
                o.RegisteredAt
            );
    }
}
