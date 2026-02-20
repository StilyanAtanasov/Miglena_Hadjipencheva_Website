using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.Utils.Security;
using System.Security.Claims;

namespace MHAuthorWebsite.Web.Utils.Middleware;

public class LegalDocumentsAccessMiddleware
{
    private readonly RequestDelegate _next;

    public LegalDocumentsAccessMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContext context, ILegalDocumentsService legalDocumentsService)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            string path = context.Request.Path.Value ?? string.Empty;
            if (!LegalAccessRules.IsPathAllowedForUnaccepted(path))
            {
                string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    bool accepted = await legalDocumentsService.HasUserAcceptedLatestDocumentsAsync(userId);
                    if (!accepted)
                    {
                        string currentUrl = $"{context.Request.Path}{context.Request.QueryString}";
                        string redirectUrl = $"/Identity/Account/Manage/LegalAgreements?prompt=1&returnUrl={Uri.EscapeDataString(currentUrl)}";
                        context.Response.Redirect(redirectUrl);
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}
