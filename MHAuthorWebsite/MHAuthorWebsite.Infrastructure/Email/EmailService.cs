using MHAuthorWebsite.Core.Configuration.EmailConfiguration;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings) => _settings = settings.Value;

    public async Task SendEmailAsync(IEmailUser from, string to, string subject, string body, bool isBodyHtml)
    {
        using SmtpClient client = new(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(from.Username, from.Password)
        };

        MailAddress fromAddress = new(from.Username, WebsiteName);
        MailAddress toAddress = new(to);

        using MailMessage message = new(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };

        message.Headers.Add(
            "Message-ID",
            $"<{Guid.NewGuid()}@miglena-hadjipencheva.com>"
        );

        await client.SendMailAsync(message);
    }

    public async Task SendEmailsBulkAsync(IEmailUser from, ICollection<string> to, string subject, string body, bool isBodyHtml)
    {
        IEnumerable<Task> sendTasks = to
            .Select(adminEmail => SendEmailAsync(from, adminEmail, subject, body, isBodyHtml));

        await Task.WhenAll(sendTasks);
    }
}
