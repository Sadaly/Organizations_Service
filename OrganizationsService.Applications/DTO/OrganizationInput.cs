namespace OrganizationsService.Applications.DTO
{
    /// <summary>
    /// Data for creating or updating an organization.
    /// </summary>
    /// <param name="Name">
    /// The name of the organization.
    /// <example>Example LLC</example>
    /// </param>
    /// <param name="Tin">
    /// The taxpayer identification number (TIN).
    /// <example>2310123456</example>
    /// </param>
    /// <param name="Description">
    /// A description of the organization.
    /// <example>Supplier of office supplies</example>
    /// </param>
    /// <param name="Address">
    /// The organization's address.
    /// <example>Krasnodar, Krasnaya 10</example>
    /// </param>
    /// <param name="Phone">
    /// The organization's contact phone number.
    /// <example>+78001234567</example>
    /// </param>
    /// <param name="Email">
    /// The organization's contact email address.
    /// <example>info@example.com</example>
    /// </param>
    /// <param name="RegisteredAt">
    /// The organization's registration date.
    /// <example>2026-01-15</example>
    /// </param>
    public sealed record OrganizationInput(
        string Name,
        string Tin,
        string? Description,
        string? Address,
        string? Phone,
        string? Email,
        DateOnly RegisteredAt);
}
