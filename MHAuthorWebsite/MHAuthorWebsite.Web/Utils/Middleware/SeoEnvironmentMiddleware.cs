namespace MHAuthorWebsite.Web.Utils.Middleware;

public class SeoEnvironmentMiddleware
{
    private const string RobotsPath = "/robots.txt";
    private const string SitemapPath = "/sitemap.xml";

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SeoEnvironmentMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_environment.IsStaging())
        {
            await _next(context);
            return;
        }

        string path = context.Request.Path.Value ?? string.Empty;
        if (path.Equals(RobotsPath, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("User-agent: *\nDisallow: /");
            return;
        }

        if (path.Equals(SitemapPath, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType = "application/xml; charset=utf-8";
            await context.Response.WriteAsync(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"></urlset>");
            return;
        }

        await _next(context);
    }
}
