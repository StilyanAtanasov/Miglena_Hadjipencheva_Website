using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices.Admin;

public class AdminOrderDataService : IAdminOrderDataService
{
    private readonly IApplicationRepository _repository;

    public AdminOrderDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Order?> GetOrderByIdForOrderDetailsReadonlyAsync(Guid orderId)
    => await _repository
        .WhereReadonly<Order>(o => o.Id == orderId)
        .Include(o => o.OrderedProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Thumbnail)
                    .ThenInclude(t => t.Image)
        .Include(o => o.Shipment)
            .ThenInclude(s => s.Events)
        .Include(o => o.Shipment)
            .ThenInclude(s => s.Services)
        .FirstOrDefaultAsync();

    public async Task<Order?> GetOrderByIdForEditAndOrderDtoAsync(Guid orderId)
    => await _repository
        .All<Order>()
        .Include(o => o.Shipment)
        .Include(o => o.OrderedProducts)
            .ThenInclude(op => op.Product)
        .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<OrderProduct[]> GetOrderProductsByOrderByIdForRestoringProductAsync(Guid orderId)
    => await _repository
       .Where<OrderProduct>(op => op.OrderId == orderId)
       .Include(op => op.Product)
       .ToArrayAsync();
}