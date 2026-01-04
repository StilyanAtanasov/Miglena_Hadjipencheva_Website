using MHAuthorWebsite.Web.Utils.Builders;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MHAuthorWebsite.Web.Utils.Attributes;

public class SecurityHeadersAttribute : ActionFilterAttribute
{
    private readonly CspFeature _features;

    public SecurityHeadersAttribute(CspFeature features = 0) => _features = features;

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        string policy = CspPolicyBuilder.Build(_features);
        HttpResponse response = context.HttpContext.Response;

        response.Headers["Content-Security-Policy"] = policy;

        response.Headers["X-Content-Type-Options"] = "nosniff";
        response.Headers["X-Frame-Options"] = "SAMEORIGIN";
        response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    }
}