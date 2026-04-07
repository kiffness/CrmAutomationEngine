namespace CrmAutomationEngine.Desktop.Models;

public record EmailTemplate(Guid Id, string Name, string HtmlBody, DateTime CreatedAt, DateTime UpdatedAt);