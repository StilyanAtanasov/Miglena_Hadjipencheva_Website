using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;

namespace MHAuthorWebsite.Core.Configuration.EmailConfiguration.EmailUsers;

public class ContactEmailUser : IEmailUser
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}