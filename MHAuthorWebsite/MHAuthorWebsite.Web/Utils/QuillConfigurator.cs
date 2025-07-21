using Ganss.Xss;

namespace MHAuthorWebsite.Web.Utils;

public static class QuillConfigurator
{
    public static HtmlSanitizer GetQuillHtmlSanitizer()
    {
        return new HtmlSanitizer
        {
            AllowDataAttributes = true,
        };
    }
}