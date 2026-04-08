using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Core.Enums;
using CrmAutomationEngine.Core.Interfaces;
using CrmAutomationEngine.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Jobs;

public class ContactSyncJob(AppDbContext db, IHubSpotClient hubSpotClient, IBackgroundJobClient jobClient)
{
    public async Task RunAsync(CancellationToken ct)
    {
        var tenants = await db.Tenants.ToListAsync(ct);

        foreach (var tenant in tenants)
        {
            var hubSpotContacts = await hubSpotClient.GetContactsAsync(tenant.HubSpotToken, ct);

            var existingContacts = await db.Contacts
                .IgnoreQueryFilters()
                .Where(c => c.TenantId == tenant.Id)
                .ToListAsync(ct);

            var automations = await db.Automations
                .IgnoreQueryFilters()
                .Where(a => a.TenantId == tenant.Id && a.Trigger == TriggerType.ContactCreated)
                .ToListAsync(ct);

            foreach (var incoming in hubSpotContacts)
            {
                var existing = existingContacts.FirstOrDefault(c => c.HubSpotId == incoming.HubSpotId);
                if (existing is null)
                {
                    incoming.Id = Guid.NewGuid();
                    incoming.TenantId = tenant.Id;
                    db.Contacts.Add(incoming);

                    await db.SaveChangesAsync(ct);

                    var existingEnrolmentAutomationIds = await db.AutomationEnrolments
                        .IgnoreQueryFilters()
                        .Where(e => e.TenantId == tenant.Id && e.ContactId == incoming.Id)
                        .Select(e => e.AutomationId)
                        .ToListAsync(ct);

                    foreach (var automation in automations.Where(a => !existingEnrolmentAutomationIds.Contains(a.Id)))
                        EnqueueAutomation(tenant.Id, automation, incoming.Id);
                }
                else
                {
                    existing.FirstName = incoming.FirstName;
                    existing.LastName = incoming.LastName;
                    existing.Email = incoming.Email;
                    existing.CompanyName = incoming.CompanyName;
                    existing.LastSyncedAt = DateTime.UtcNow;
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }

    private void EnqueueAutomation(Guid tenantId, Automation automation, Guid contactId)
    {
        var enrolment = new AutomationEnrolment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AutomationId = automation.Id,
            ContactId = contactId,
            Status = EnrolmentStatus.Pending,
            EnrolledAt = DateTime.UtcNow
        };
        db.AutomationEnrolments.Add(enrolment);
        db.SaveChanges();

        string hangfireJobId = automation.DelayMinutes > 0
            ? jobClient.Schedule<SendEmailJob>(j => j.RunAsync(enrolment.Id, CancellationToken.None), TimeSpan.FromMinutes(automation.DelayMinutes))
            : jobClient.Enqueue<SendEmailJob>(j => j.RunAsync(enrolment.Id, CancellationToken.None));

        db.ScheduledJobs.Add(new ScheduledJob
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            HangfireJobId = hangfireJobId,
            JobType = "SendEmail",
            Status = JobStatus.Scheduled,
            EnrolmentId = enrolment.Id,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}
