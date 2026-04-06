using CrmAutomationEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrmAutomationEngine.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.HubSpotId).IsRequired().HasMaxLength(64);
        builder.Property(e => e.FirstName).HasMaxLength(128);
        builder.Property(e => e.LastName).HasMaxLength(128);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.CompanyName).HasMaxLength(256);
        builder.HasIndex(e => new { e.TenantId, e.HubSpotId }).IsUnique();
    }
}
