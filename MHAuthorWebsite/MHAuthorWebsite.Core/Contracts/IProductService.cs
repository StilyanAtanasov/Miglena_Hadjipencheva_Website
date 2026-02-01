using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Models;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductService
{
    Task<ICollection<ProductCardDto>> GetAllProductCardsReadonlyAsync(string? userId, int page,
        (bool descending, Expression<Func<Product, object>>? expression) sortType, string? searchString);

    Task<int> GetAllProductsCountAsync(string? searchString);

    Task<ServiceResult<ProductDetailsDto>> GetProductDetailsReadonlyAsync(Guid productId, string? userId);

    Task<ICollection<LikedProductDto>> GetLikedProductsReadonlyAsync(string userId);

    Task<ServiceResult> ToggleLikeProduct(string userId, Guid productId);
}