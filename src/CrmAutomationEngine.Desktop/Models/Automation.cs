namespace CrmAutomationEngine.Desktop.Models;

public record Automation(Guid Id, string Name, int Trigger, Guid EmailTemplateId, EmailTemplate? EmailTemplate, int DelayMinutes);
