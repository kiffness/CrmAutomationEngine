using CrmAutomationEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrmAutomationEngine.Infrastructure.Persistence.Configurations;

public class AutomationEnrolmentConfiguration : IEntityTypeConfiguration<AutomationEnrolment>
{
    public void Configure(EntityTypeBuilder<AutomationEnrolment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Automation)
               .WithMany()
               .HasForeignKey(e => e.AutomationId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Contact)
               .WithMany()
               .HasForeignKey(e => e.ContactId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
