using CrmAutomationEngine.Core.Enums;

namespace CrmAutomationEngine.Core.Entities;

public class Automation : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public TriggerType Trigger { get; set; }
    public Guid EmailTemplateId { get; set; }
    public EmailTemplate EmailTemplate { get; set; } = null!;
    public int DelayMinutes { get; set; }  // 0 = fire immediately
}
