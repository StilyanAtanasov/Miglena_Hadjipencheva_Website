namespace MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;

public interface IEmailUserProvider
{
    IEmailUser GetContactUser();

    IEmailUser GetNotificationsUser();
}