using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.EmailConfiguration;
using MHAuthorWebsite.Core.EmailConfiguration.Contracts;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Core;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings) => _settings = settings.Value;

    public async Task SendEmailAsync(IEmailUser from, string to, string subject, string body, bool isBodyHtml)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(from.Username, from.Password)
        };

        MailAddress fromAddress = new(from.Username, WebsiteName);
        MailAddress toAddress = new(to);

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };

        await client.SendMailAsync(message);
    }
}