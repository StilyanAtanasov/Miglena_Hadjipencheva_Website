using MHAuthorWebsite.Web.Utils.Enums;

namespace MHAuthorWebsite.Web.Utils.Builders;

public static class CspPolicyBuilder
{
    public static string Build(CspFeature features)
    {
        HashSet<string> scripts = new() { "'self'", "https://site-assets.fontawesome.com" };
        HashSet<string> styles = new() { "'self'", "https://fonts.googleapis.com", "https://site-assets.fontawesome.com" };
        HashSet<string> connects = new() { "'self'", "ws:", "wss:", "http://localhost:*", "https://localhost:*" };
        HashSet<string> fonts = new() { "'self'", "https://fonts.gstatic.com", "https://site-assets.fontawesome.com" };
        HashSet<string> frames = new() { "'self'" };
        HashSet<string> frameAncestors = new() { "'self'" };
        HashSet<string> images = new() { "'self'", "data:", "https://res.cloudinary.com" };

        if (features.HasFlag(CspFeature.Econt))
        {
            frames.Add("https://delivery.econt.com");
            frames.Add("https://ee.econt.com");
            frameAncestors.Add("https://delivery.econt.com");
            frameAncestors.Add("https://delivery-demo.econt.com");
        }

        if (features.HasFlag(CspFeature.Editor))
        {
            scripts.Add("https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.js");
            scripts.Add("https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js");
            scripts.Add("https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/katex.min.js");

            styles.Add("https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.snow.css");
            styles.Add("https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css");
            styles.Add("https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/katex.min.css");

            fonts.Add("https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/fonts/");

            connects.Add("https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/");
        }

        if (features.HasFlag(CspFeature.Notifications))
        {
            scripts.Add("https://cdn.jsdelivr.net/npm/sweetalert2@11.22.3/dist/sweetalert2.esm.js");
            styles.Add("https://cdn.jsdelivr.net/npm/sweetalert2@11.22.3/dist/sweetalert2.min.css");
        }

        if (features.HasFlag(CspFeature.QrCodes))
        {
            scripts.Add("https://cdn.jsdelivr.net/npm/qrcodejs@1.0.0/qrcode.min.js");
        }

        if (features.HasFlag(CspFeature.TomSelect))
        {
            connects.Add("https://cdn.jsdelivr.net/npm/tom-select@2.4.3/dist/");

            scripts.Add("https://cdn.jsdelivr.net/npm/tom-select@2.4.3/dist/js/tom-select.complete.min.js");
            styles.Add("https://cdn.jsdelivr.net/npm/tom-select@2.4.3/dist/css/tom-select.css");
        }

        return string.Join(" ", new[] {
            "default-src 'self';",
            $"script-src {string.Join(" ", scripts)};",
            $"style-src {string.Join(" ", styles)};",
            $"font-src {string.Join(" ", fonts)};",
            $"img-src {string.Join(" ", images)};",
            $"frame-src {string.Join(" ", frames)};",
            $"connect-src {string.Join(" ", connects)};",
            $"frame-ancestors {string.Join(" ", frameAncestors)};"
        });
    }
}