using Ganss.Xss;

namespace MHAuthorWebsite.Web.Utils;

public static class QuillConfigurator
{
    public static HtmlSanitizer GetQuillHtmlSanitizer()
    {
        return new HtmlSanitizer
        {
            AllowedTags =
            {
                "p", "br", "strong", "em", "ul", "ol", "li", "a", "img",
                "h1", "h2", "h3", "h4", "h5", "h6"
            },
            AllowDataAttributes = true,
        };
    }
}