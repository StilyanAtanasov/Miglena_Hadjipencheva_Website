using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Admin.Contracts.DataServices;

public interface ICloudinaryAdminProductImageDataService
{
    Task<Product?> GetNonDeletedProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<Product> GetNonDeletedProductForTitleImageUpdateByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<ProductImage?> GetProductImageByIdAsync(Guid imageId, CancellationToken cancellationToken = default);

    Task<ProductImage?> GetProductImageForTitleImageUpdateByIdAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default);
}
