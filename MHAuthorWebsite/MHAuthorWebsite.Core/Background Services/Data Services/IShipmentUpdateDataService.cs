using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Background_Services.Data_Services;

public interface IShipmentUpdateDataService
{
    Task<Order[]> GetOrdersOnTheWayReadonlyAsync(CancellationToken ct);
}