using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class ShipmentUpdateDataService : IShipmentUpdateDataService
{
    private readonly IApplicationRepository _repository;

    public ShipmentUpdateDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Order[]> GetOrdersOnTheWayReadonlyAsync(CancellationToken ct)
    => await _repository
        .WhereReadonly<Order>(o => o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Accepted)
        .Include(o => o.Shipment)
        .ThenInclude(s => s.Events)
        .Include(o => o.User)
        .ToArrayAsync(ct);
}