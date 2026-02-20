namespace MHAuthorWebsite.Web.Utils.Security;

public static class LegalAccessRules
{
    private static readonly string[] AllowedPrefixes =
    {
        "/Identity/Account/Login",
        "/Identity/Account/Logout",
        "/Identity/Account/Manage/Index",
        "/Identity/Account/Manage/PersonalData",
        "/Identity/Account/Manage/DeletePersonalData",
        "/Identity/Account/Manage/DownloadPersonalData",
        "/Identity/Account/Manage/LegalAgreements",
        "/Home/PrivacyPolicy",
        "/Home/TermsOfService",
        "/Error"
    };

    public static bool IsPathAllowedForUnaccepted(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        foreach (string prefix in AllowedPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
