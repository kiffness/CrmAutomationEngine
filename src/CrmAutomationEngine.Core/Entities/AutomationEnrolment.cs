using CrmAutomationEngine.Core.Enums;

namespace CrmAutomationEngine.Core.Entities;

public class AutomationEnrolment : TenantEntity
{
    public Guid AutomationId { get; set; }
    public Automation Automation { get; set; } = null!;
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
    public EnrolmentStatus Status { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
