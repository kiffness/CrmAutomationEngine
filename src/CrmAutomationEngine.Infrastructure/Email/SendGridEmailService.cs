using CrmAutomationEngine.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CrmAutomationEngine.Infrastructure.Email;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;

    public SendGridEmailService(IConfiguration config)
    {
        _apiKey = config["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid:ApiKey not configured");
        _fromEmail = config["SendGrid:FromEmail"] ?? throw new InvalidOperationException("SendGrid:FromEmail not configured");
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var client = new SendGridClient(_apiKey);
        var msg = MailHelper.CreateSingleEmail(
            new EmailAddress(_fromEmail),
            new EmailAddress(toEmail),
            subject,
            plainTextContent: null,
            htmlContent: htmlBody);

        var response = await client.SendEmailAsync(msg, ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"SendGrid returned {response.StatusCode}");
    }
}
