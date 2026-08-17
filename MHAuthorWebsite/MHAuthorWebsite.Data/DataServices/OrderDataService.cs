using MHAuthorWebsite.Core.Common.Extensions;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Order;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Order;

namespace MHAuthorWebsite.Data.DataServices;

public class OrderDataService : IOrderDataService
{
    private readonly IApplicationRepository _repository;

    public OrderDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<MyOrderDto[]> GetUserOrdersPagedReadonlyAsync(string userId, int page)
        => await _repository
            .WhereReadonly<Order>(o => o.UserId == userId)
            .IgnoreQueryFilters()
            .OrderByDescending(o => o.Date)
            .Skip((page - 1) * MyOrdersPageSize)
            .Take(MyOrdersPageSize)
            .Select(o => new MyOrderDto
            {
                OrderId = o.Id,
                CreatedAt = o.Date,
                Total = o.OrderedProducts.Sum(op => op.UnitPrice * op.Quantity) + o.Shipment.ShippingPrice,
                Currency = o.Shipment.Currency,
                Status = o.Status.GetDisplayName(),
                Products = o.OrderedProducts
                    .Select(op => new MyOrdersOrderProductDto
                    {
                        ImageUrl = op.Product.Thumbnail.Image.ImageUrl,
                        Quantity = op.Quantity,
                    })
                    .ToArray()
            })
            .ToArrayAsync();

    public async Task<CartItem[]> GetOrderCartItemsByUserId(string userId)
     => await _repository
         .Where<CartItem>(ci => ci.Cart.UserId == userId && ci.IsSelected && ci.Product.IsPublic && ci.Product.StockQuantity >= ci.Quantity)
         .Include(ci => ci.Cart)
         .Include(ci => ci.Product)
            .ThenInclude(p => p.Discounts)
         .ToArrayAsync();

    public async Task<Order?> GetOrderByIdForOrderDetails(Guid orderId)
     => await _repository
         .WhereReadonly<Order>(o => o.Id == orderId)
         .IgnoreQueryFilters()
         .Include(o => o.OrderedProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Thumbnail)
                    .ThenInclude(t => t.Image)
         .Include(o => o.Shipment)
            .ThenInclude(s => s.Events)
         .FirstOrDefaultAsync();
}