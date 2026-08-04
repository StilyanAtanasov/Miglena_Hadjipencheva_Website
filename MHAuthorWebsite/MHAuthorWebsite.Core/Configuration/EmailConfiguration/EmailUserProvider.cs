using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using Microsoft.Extensions.Options;

namespace MHAuthorWebsite.Core.Configuration.EmailConfiguration;

public class EmailUserProvider : IEmailUserProvider
{
    private readonly EmailSettings _settings;

    public EmailUserProvider(IOptions<EmailSettings> settings) => _settings = settings.Value;

    public IEmailUser GetContactUser() => _settings.ContactEmailUser;

    public IEmailUser GetNotificationsUser() => _settings.NotificationsEmailUser;
}