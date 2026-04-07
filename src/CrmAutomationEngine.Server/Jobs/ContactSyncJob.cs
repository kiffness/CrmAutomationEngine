using CrmAutomationEngine.Core.Interfaces;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Jobs;

public class ContactSyncJob(AppDbContext db, IHubSpotClient hubSpotClient)
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

            foreach (var incoming in hubSpotContacts)
            {
                var existing = existingContacts.FirstOrDefault(c => c.HubSpotId == incoming.HubSpotId);
                if (existing is null)
                {
                    incoming.Id = Guid.NewGuid();
                    incoming.TenantId = tenant.Id;
                    db.Contacts.Add(incoming);
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
}
