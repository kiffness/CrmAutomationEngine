namespace CrmAutomationEngine.Core.Entities;

public class EmailTemplate : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;  // contains {{firstName}} etc.
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
