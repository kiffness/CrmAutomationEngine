using CrmAutomationEngine.Core.Enums;

namespace CrmAutomationEngine.Core.Entities;

public class ScheduledJob
{
    public string HangfireJobId { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;   // e.g. "SendEmail", "ContactSync"
    public JobStatus Status { get; set; }
    public Guid? EnrolmentId { get; set; }
    public AutomationEnrolment? Enrolment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
