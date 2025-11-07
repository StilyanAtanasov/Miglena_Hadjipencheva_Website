namespace MHAuthorWebsite.Core.EmailConfiguration.Contracts;

public interface IEmailUserProvider
{
    IEmailUser GetContactUser();
}