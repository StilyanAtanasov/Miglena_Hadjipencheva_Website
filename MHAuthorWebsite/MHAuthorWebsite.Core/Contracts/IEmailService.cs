using MHAuthorWebsite.Core.EmailConfiguration.Contracts;

namespace MHAuthorWebsite.Core.Contracts;

public interface IEmailService
{
    Task SendEmailAsync(IEmailUser from, string to, string subject, string body, bool isBodyHtml);
}