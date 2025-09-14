namespace MHAuthorWebsite.Web.Utils;

public static class OAuthProviderMapper
{
    public static Dictionary<string, string> ProviderIcons { get; } = new()
    {
        { "Google", "~/img/icons/google.svg" }
    };
}