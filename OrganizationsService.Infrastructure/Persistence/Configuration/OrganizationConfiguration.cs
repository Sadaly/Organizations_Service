using Microsoft.EntityFrameworkCore;
using OrganizationsService.Domain.Entities;

namespace OrganizationsService.Infrastructure.Persistence.Configuration
{
    internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("Organizations");
            builder.HasKey(o => o.Id).HasName("id");
            builder.Property(o => o.Id).HasColumnName("id");
            builder.Property(o => o.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(o => o.Tin).HasColumnName("tin").IsRequired().HasMaxLength(20);
            builder.Property(o => o.Description).HasColumnName("description").HasMaxLength(500);
            builder.Property(o => o.Address).HasColumnName("address").HasMaxLength(200);
            builder.Property(o => o.Phone).HasColumnName("phone").HasMaxLength(20);
            builder.Property(o => o.Email).HasColumnName("email").HasMaxLength(100);
            builder.Property(o => o.RegisteredAt).HasColumnName("registered_at").IsRequired();

            // Add a unique index on the Tin property. Since we added search by tin.
            builder.HasIndex(o => o.Tin).HasDatabaseName("ux_organizations_tin").IsUnique();
        }
    }
}
