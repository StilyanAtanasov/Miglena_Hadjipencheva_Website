using MHAuthorWebsite.Core.Dtos.Admin.Product;
using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Admin.Contracts.DataServices;

public interface IAdminProductDataService
{
    Task<ICollection<Guid>> GetImageIdsByProductId(Guid productId);

    Task<Product?> GetNonDeletedProductByIdAsync(Guid productId);

    Task<Product?> GetProductForUpdateByIdAsync(Guid productId);

    Task<Product?> GetProductForEditByIdReadonlyAsync(Guid productId);

    Task<ICollection<ProductListItemDto>> GetProductsListReadonlyAsync();
}