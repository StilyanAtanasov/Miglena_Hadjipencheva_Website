namespace MHAuthorWebsite.Core.EmailConfiguration.Contracts;

public interface IEmailUser
{
    public string Username { get; set; }

    public string Password { get; set; }
}