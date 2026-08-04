using MHAuthorWebsite.Core.Dtos.Product;
using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts.DataServices;

public interface IProductDataService
{
    Task<Product?> GetProductWithLikesForEditByIdAsync(Guid productId);

    Task<ProductDetailsGeneralInfoDto?> GetProductDetailsGeneralInfoByIdAsync(Guid productId, bool includeNonPublicProducts);

    Task<ProductDetailsCommentsInfoDto?> GetProductDetailsCommentsInfoByIdAsync(Guid productId, bool includeNonPublicProducts, HashSet<string> adminIdSet);

    Task<ProductDetailsUserInfoDto?> GetProductDetailsUserInfoByIdAsync(Guid productId, bool includeNonPublicProducts, string userId, Guid commitId);
}