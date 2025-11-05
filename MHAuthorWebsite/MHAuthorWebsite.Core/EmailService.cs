using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.EmailConfiguration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace MHAuthorWebsite.Core;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings) => _settings = settings.Value;

    public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml)
    {
        SmtpClient client = new(_settings.Host, _settings.Port);
        client.EnableSsl = _settings.UseSsl;
        client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

        MailMessage message = new(_settings.Username, to, subject, body);
        message.IsBodyHtml = isBodyHtml;

        await client.SendMailAsync(message);
    }
}