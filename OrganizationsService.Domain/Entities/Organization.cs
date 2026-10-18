namespace OrganizationsService.Domain.Entities
{
    /// <summary>
    /// Stores organization information.
    /// </summary>
    /// <remarks> Also could use Value objects for TIN, Phone, Address, etc. But decided to use C# types</remarks>
    public class Organization
    {
        /// <summary>
        /// Unique Identifier for the organization.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Organization name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Tax Identification Number.
        /// </summary>
        /// <remarks> String value, cause TINs can contain leading zeros. </remarks> 
        public string Tin { get; set; } = string.Empty;
        /// <summary>
        /// Description of the organization.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Address of the organization.
        /// </summary>
        public string? Address { get; set; } = string.Empty;
        /// <summary>
        /// Phone number of the organization.
        /// </summary>
        public string? Phone { get; set; } = string.Empty;
        /// <summary>
        /// Email address of the organization.
        /// </summary>
        public string? Email { get; set; } = string.Empty;
        /// <summary>
        /// Registration date of the organization.
        /// </summary>
        public DateOnly RegisteredAt { get; set; }
    }
}
