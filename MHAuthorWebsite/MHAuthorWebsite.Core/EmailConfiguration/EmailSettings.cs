namespace MHAuthorWebsite.Core.EmailConfiguration;

public class EmailSettings
{
    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public bool UseSsl { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}