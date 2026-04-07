namespace CrmAutomationEngine.Desktop.Models;

public record ScheduledJob(
        Guid Id, 
        string HangfireJobId, 
        string JobType, 
        int Status, 
        Guid? EnrolmentId, 
        DateTime CreatedAt, 
        DateTime? CompletedAt, 
        string? ErrorMessage
    );