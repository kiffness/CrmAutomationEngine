using CrmAutomationEngine.Core.Enums;
using CrmAutomationEngine.Core.Interfaces;
using CrmAutomationEngine.Infrastructure.Persistence;
using CrmAutomationEngine.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Jobs;

public class SendEmailJob(AppDbContext db, IEmailService emailService, TemplateRenderer renderer)
{
    public async Task RunAsync(Guid enrolmentId, CancellationToken ct)
    {
        var enrolment = await db.AutomationEnrolments
                            .IgnoreQueryFilters()
                            .Include(e => e.Automation).ThenInclude(a => a.EmailTemplate)
                            .Include(e => e.Contact)
                            .FirstOrDefaultAsync(e => e.Id == enrolmentId, ct)
                        ?? throw new InvalidOperationException($"Enrolment {enrolmentId} not found");

        var job = await db.ScheduledJobs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(j => j.EnrolmentId == enrolmentId, ct);

        try
        {
            var html = renderer.Render(enrolment.Automation.EmailTemplate.HtmlBody, enrolment.Contact);
            await emailService.SendAsync(enrolment.Contact.Email, enrolment.Automation.Name, html, ct);

            enrolment.Status = EnrolmentStatus.Completed;
            enrolment.CompletedAt = DateTime.UtcNow;
            if (job is not null) { job.Status = JobStatus.Completed; job.CompletedAt = DateTime.UtcNow; }
        }
        catch (Exception ex)
        {
            enrolment.Status = EnrolmentStatus.Failed;
            if (job is not null) { job.Status = JobStatus.Failed; job.ErrorMessage = ex.Message; }
            throw; // rethrow so Hangfire marks the job as failed and can retry
        }
        finally
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
