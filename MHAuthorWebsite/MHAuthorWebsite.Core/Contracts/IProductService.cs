using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Web.ViewModels.Product;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductService
{
    Task<ServiceResult> AddProductAsync(AddProductForm model);

    // Task<ServiceResult> UpdateProductAsync(UpdateProductForm model);

    // Task<ServiceResult> DeleteProductAsync(int productId);

    // Task<ServiceResult<ICollection<ProductDto>>> GetAllProductsAsync();

    Task<ICollection<ProductTypeAttributesDto>> GetProductTypeAttributesAsync(int productTypeId);
}