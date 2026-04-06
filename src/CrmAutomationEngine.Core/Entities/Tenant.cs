namespace CrmAutomationEngine.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string HubSpotToken { get; set; } = string.Empty;
    public string HubSpotClientSecret { get; set; } = string.Empty;
}
