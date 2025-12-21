using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Admin.Contracts.DataServices;

public interface ICloudinaryAdminProductImageDataService
{
    Task<Product?> GetNonDeletedProductByIdAsync(Guid productId);

    Task<Product> GetNonDeletedProductForTitleImageUpdateByIdAsync(Guid productId);

    Task<ProductImage?> GetProductImageByIdAsync(Guid imageId);

    Task<ProductImage?> GetProductImageForTitleImageUpdateByIdAsync(Guid productId, Guid imageId);
}