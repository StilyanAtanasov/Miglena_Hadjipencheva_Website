using MHAuthorWebsite.Core.EmailConfiguration.Contracts;

namespace MHAuthorWebsite.Core.EmailConfiguration.EmailUsers;

public class NotificationsEmailUser : IEmailUser
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}