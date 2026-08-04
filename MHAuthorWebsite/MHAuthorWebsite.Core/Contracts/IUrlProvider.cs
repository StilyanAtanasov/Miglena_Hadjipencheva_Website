namespace MHAuthorWebsite.Core.Contracts;

public interface IUrlProvider
{
    string GetOrderDetailsPageUrl(Guid orderId);

    string GetAdminOrderDetailsPageUrl(Guid orderId);

    string GetContactsPageUrl();

    string GetContactRequestPageUrl(Guid requestId);

    string GetMarketingUnsubscribeUrl(string email, string token);
}
