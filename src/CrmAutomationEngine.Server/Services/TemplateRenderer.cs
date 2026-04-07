using CrmAutomationEngine.Core.Entities;

namespace CrmAutomationEngine.Server.Services;

public class TemplateRenderer
{
    public string Render(string htmlBody, Contact contact) =>
        htmlBody
            .Replace("{{firstName}}", contact.FirstName)
            .Replace("{{lastName}}", contact.LastName)
            .Replace("{{email}}", contact.Email)
            .Replace("{{companyName}}", contact.CompanyName);
}
