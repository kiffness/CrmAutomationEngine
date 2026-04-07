using CrmAutomationEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrmAutomationEngine.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ApiKey).IsRequired().HasMaxLength(128);
        builder.HasIndex(e => e.ApiKey).IsUnique();
        builder.Property(e => e.HubSpotToken).IsRequired().HasMaxLength(512);
        builder.Property(e => e.HubSpotClientSecret).IsRequired().HasMaxLength(256);
        builder.Property(e => e.AdminEmail).IsRequired().HasMaxLength(256);
        builder.HasIndex(e => e.AdminEmail).IsUnique();
        builder.Property(e => e.PasswordHash).IsRequired();
    }
}
