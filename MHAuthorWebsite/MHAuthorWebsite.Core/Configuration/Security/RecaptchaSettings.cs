namespace MHAuthorWebsite.Core.Configuration.Security;

public sealed class RecaptchaSettings
{
    public string V2SiteKey { get; set; } = string.Empty;

    public string V2SecretKey { get; set; } = string.Empty;

    public string V3SiteKey { get; set; } = string.Empty;

    public string V3SecretKey { get; set; } = string.Empty;

    public double V3MinimumScore { get; set; } = 0.5;
}
