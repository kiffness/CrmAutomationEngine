using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Core.Enums;
using CrmAutomationEngine.Core.Interfaces;
using CrmAutomationEngine.Infrastructure.Persistence;
using CrmAutomationEngine.Server.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    [HttpPost("{apiKey}")]
    public async Task<IActionResult> Receive(
        string apiKey,
        [FromServices] AppDbContext db,
        [FromServices] TenantContext tenantContext,
        [FromServices] IHubSpotClient hubSpotClient,
        [FromServices] IBackgroundJobClient jobClient)
    {
        // 1. Read raw body
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        // 2. Validate HubSpot HMAC-SHA256 signature
        var signature = Request.Headers["X-HubSpot-Signature"].FirstOrDefault();
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.ApiKey == apiKey);
        if (tenant is null || !ValidateSignature(rawBody, signature, tenant.HubSpotClientSecret))
            return Unauthorized();

        tenantContext.TenantId = tenant.Id;

        // 3. Parse events (HubSpot sends an array)
        var events = JsonSerializer.Deserialize<List<HubSpotWebhookEvent>>(rawBody);
        if (events is null) return BadRequest();

        foreach (var evt in events)
        {
            var triggerType = evt.SubscriptionType switch
            {
                "contact.creation" => TriggerType.ContactCreated,
                "deal.propertyChange" => TriggerType.DealStageChanged,
                _ => (TriggerType?)null
            };
            if (triggerType is null) continue;

            // 4. Find matching automation
            var automation = await db.Automations
                .FirstOrDefaultAsync(a => a.Trigger == triggerType);
            if (automation is null) continue;

            // 5. Upsert contact
            var contact = await db.Contacts.FirstOrDefaultAsync(c => c.HubSpotId == evt.ObjectId.ToString());
            if (contact is null)
            {
                var fetched = await hubSpotClient.GetContactAsync(tenant.HubSpotToken, evt.ObjectId.ToString());
                if (fetched is null) continue;

                fetched.Id = Guid.NewGuid();
                fetched.TenantId = tenant.Id;
                db.Contacts.Add(fetched);
                contact = fetched;
            }

            // 6. Create enrolment record
            var enrolment = new AutomationEnrolment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                AutomationId = automation.Id,
                ContactId = contact.Id,
                Status = EnrolmentStatus.Pending,
                EnrolledAt = DateTime.UtcNow
            };
            db.AutomationEnrolments.Add(enrolment);
            await db.SaveChangesAsync();

            // 7. Enqueue Hangfire job (with optional delay)
            string hangfireJobId;
            if (automation.DelayMinutes > 0)
                hangfireJobId = jobClient.Schedule(
                    (Expression<Func<SendEmailJob, Task>>)(j => j.RunAsync(enrolment.Id, CancellationToken.None)),
                    TimeSpan.FromMinutes(automation.DelayMinutes));
            else
                hangfireJobId = jobClient.Enqueue(
                    (Expression<Func<SendEmailJob, Task>>)(j => j.RunAsync(enrolment.Id, CancellationToken.None)));

            // 8. Record ScheduledJob
            db.ScheduledJobs.Add(new ScheduledJob
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                HangfireJobId = hangfireJobId,
                JobType = "SendEmail",
                Status = JobStatus.Scheduled,
                EnrolmentId = enrolment.Id,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        return Ok();
    }

    private static bool ValidateSignature(string body, string? signature, string secret)
    {
        if (signature is null) return false;
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();
        return expected.Equals(signature, StringComparison.InvariantCultureIgnoreCase);
    }
}

public record HubSpotWebhookEvent(string SubscriptionType, long ObjectId);
