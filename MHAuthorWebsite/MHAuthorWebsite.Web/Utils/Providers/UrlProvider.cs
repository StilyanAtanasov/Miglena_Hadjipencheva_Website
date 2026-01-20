using MHAuthorWebsite.Core.Contracts;

namespace MHAuthorWebsite.Web.Utils.Providers;

public class UrlProvider : IUrlProvider
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UrlProvider(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetOrderDetailsPageUrl(Guid orderId)
    => _linkGenerator.GetUriByAction(
            _httpContextAccessor.HttpContext!,
            action: "OrderDetails",
            controller: "Order",
            values: new { orderId = orderId }) ?? string.Empty;

    public string GetContactsPageUrl()
    => _linkGenerator.GetUriByAction(
            httpContext: _httpContextAccessor.HttpContext!,
            action: "Index",
            controller: "Contacts"
        ) ?? string.Empty;

    public string GetContactRequestPageUrl(Guid requestId)
    => _linkGenerator.GetUriByAction(
            _httpContextAccessor.HttpContext!,
            action: "ContactRequestDetails",
            controller: "AdminContactRequests",
            values: new { area = "Admin", requestId }) ?? string.Empty;
}