using CrmAutomationEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrmAutomationEngine.Infrastructure.Persistence.Configurations;

public class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJob>
{
    public void Configure(EntityTypeBuilder<ScheduledJob> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.HangfireJobId).IsRequired().HasMaxLength(128);
        builder.Property(e => e.JobType).IsRequired().HasMaxLength(64);
        builder.HasOne(e => e.Enrolment)
               .WithMany()
               .HasForeignKey(e => e.EnrolmentId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
