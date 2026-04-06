using CrmAutomationEngine.Core.Entities;

namespace CrmAutomationEngine.Core.Interfaces;

public interface IHubSpotClient
{
    Task<IReadOnlyList<Contact>> GetContactsAsync(string hubSpotToken, CancellationToken ct = default);
    Task<Contact?> GetContactAsync(string hubSpotToken, string hubSpotId, CancellationToken ct = default);
}
