namespace CrmAutomationEngine.Core.Entities;

public class Contact : TenantEntity
{
    public string HubSpotId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime LastSyncedAt { get; set; }
}
