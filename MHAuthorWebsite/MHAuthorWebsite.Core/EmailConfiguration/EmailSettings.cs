using MHAuthorWebsite.Core.EmailConfiguration.EmailUsers;

namespace MHAuthorWebsite.Core.EmailConfiguration;

public class EmailSettings
{
    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public bool UseSsl { get; set; }

    public ContactEmailUser ContactEmailUser { get; set; } = null!;
}