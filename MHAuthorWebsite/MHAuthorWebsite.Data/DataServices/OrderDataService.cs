using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class OrderDataService : IOrderDataService
{
    private readonly IApplicationRepository _repository;

    public OrderDataService(IApplicationRepository repository) => _repository = repository;

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
         .Include(o => o.OrderedProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Thumbnail)
                    .ThenInclude(t => t.Image)
         .Include(o => o.Shipment)
            .ThenInclude(s => s.Events)
         .FirstOrDefaultAsync();
}