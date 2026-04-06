using CrmAutomationEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrmAutomationEngine.Infrastructure.Persistence.Configurations;

public class AutomationConfiguration : IEntityTypeConfiguration<Automation>
{
    public void Configure(EntityTypeBuilder<Automation> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Trigger).IsRequired();
        builder.HasOne(e => e.EmailTemplate)
               .WithMany()
               .HasForeignKey(e => e.EmailTemplateId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
