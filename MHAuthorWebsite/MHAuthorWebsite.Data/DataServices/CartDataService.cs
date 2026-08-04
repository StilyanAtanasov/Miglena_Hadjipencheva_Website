using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class CartDataService : ICartDataService
{
    private readonly IApplicationRepository _repository;

    public CartDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Cart?> GetCartByUserIdReadonlyAsync(string userId)
    => await _repository
            .AllReadonly<Cart>()
            .Where(c => c.UserId == userId)
            .IgnoreQueryFilters()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductType)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Thumbnail)
                        .ThenInclude(t => t.Image)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Discounts)
            .FirstOrDefaultAsync();

    public async Task<Cart?> GetCartForItemQuantityUpdateAsync(string userId)
     => await _repository
         .All<Cart>()
         .Include(c => c.CartItems)
         .ThenInclude(ci => ci.Product)
         .ThenInclude(p => p.Discounts)
         .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Cart?> GetCartForSelectionUpdateAsync(string userId)
    => await _repository
        .All<Cart>()
        .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Product)
        .FirstOrDefaultAsync(c => c.UserId == userId);
}