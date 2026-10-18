using System.ComponentModel.DataAnnotations;
using OrganizationsService.Applications.DTO;

namespace OrganizationsService.Web.Models;

/// <summary>
/// Represents the form data used to create or update an organization.
/// </summary>
public sealed class OrganizationFormModel
{
    /// <summary>
    /// The unique identifier of the organization.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// The name of the organization.
    /// </summary>
    /// <example>Example LLC</example>
    [Required(ErrorMessage = "Organization name is required.")]
    [StringLength(100, ErrorMessage = "The name must not exceed 100 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The taxpayer identification number (TIN).
    /// </summary>
    /// <remarks>
    /// Stored as a string to preserve leading zeros.
    /// </remarks>
    /// <example>2310123456</example>
    [Required(ErrorMessage = "TIN is required.")]
    [RegularExpression(@"^\d{10}(\d{2})?$", ErrorMessage = "TIN must contain 10 or 12 digits.")]
    [Display(Name = "TIN")]
    public string Tin { get; set; } = string.Empty;

    /// <summary>
    /// A description of the organization.
    /// </summary>
    /// <example>Supplier of office supplies</example>
    [StringLength(500, ErrorMessage = "The description must not exceed 500 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// The address of the organization.
    /// </summary>
    /// <example>Krasnodar, Krasnaya 10</example>
    [StringLength(200, ErrorMessage = "The address must not exceed 200 characters.")]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    /// <summary>
    /// The contact phone number of the organization.
    /// </summary>
    /// <example>+78001234567</example>
    [Phone(ErrorMessage = "The phone number is invalid.")]
    [StringLength(20, ErrorMessage = "The phone number must not exceed 20 characters.")]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// The contact email address of the organization.
    /// </summary>
    /// <example>info@example.com</example>
    [EmailAddress(ErrorMessage = "The email address is invalid.")]
    [StringLength(100, ErrorMessage = "The email address must not exceed 100 characters.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    /// <summary>
    /// The date when the organization was registered.
    /// </summary>
    /// <example>2026-01-15</example>
    [DataType(DataType.Date)]
    [Display(Name = "Registration Date")]
    public DateOnly RegisteredAt { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Converts the form model to an organization input DTO.
    /// </summary>
    public OrganizationInput ToInput() =>
        new(Name, Tin, Description, Address, Phone, Email, RegisteredAt);

    /// <summary>
    /// Creates a form model from an organization DTO.
    /// </summary>
    /// <param name="dto">The organization DTO.</param>
    /// <returns>A form model populated with the organization's data.</returns>
    public static OrganizationFormModel FromDto(OrganizationDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Tin = dto.Tin,
        Description = dto.Description,
        Address = dto.Address,
        Phone = dto.Phone,
        Email = dto.Email,
        RegisteredAt = dto.RegisteredAt
    };
}