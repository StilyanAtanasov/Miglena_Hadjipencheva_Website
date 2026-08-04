using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.Utils.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MHAuthorWebsite.Web.Utils.Authorization;

public class LatestLegalDocumentsAcceptedHandler : AuthorizationHandler<LatestLegalDocumentsAcceptedRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILegalDocumentsService _legalDocumentsService;

    public LatestLegalDocumentsAcceptedHandler(
        IHttpContextAccessor httpContextAccessor,
        ILegalDocumentsService legalDocumentsService)
    {
        _httpContextAccessor = httpContextAccessor;
        _legalDocumentsService = legalDocumentsService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        LatestLegalDocumentsAcceptedRequirement requirement)
    {
        ClaimsPrincipal user = context.User;
        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            context.Succeed(requirement);
            return;
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        string path = httpContext?.Request.Path.Value ?? string.Empty;
        if (LegalAccessRules.IsPathAllowedForUnaccepted(path))
        {
            context.Succeed(requirement);
            return;
        }

        string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return;

        bool accepted = await _legalDocumentsService.HasUserAcceptedLatestDocumentsAsync(userId);
        if (accepted)
            context.Succeed(requirement);
    }
}
