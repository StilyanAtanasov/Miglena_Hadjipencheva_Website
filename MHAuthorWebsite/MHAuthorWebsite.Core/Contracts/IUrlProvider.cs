namespace MHAuthorWebsite.Core.Contracts;

public interface IUrlProvider
{
    string GetOrderDetailsPageUrl(Guid orderId);

    string GetContactsPageUrl();
}