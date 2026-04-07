namespace CrmAutomationEngine.Desktop.Models;

public record Contact(Guid Id, string FirstName, string LastName, string Email, string CompanyName, DateTime LastSyncedAt);