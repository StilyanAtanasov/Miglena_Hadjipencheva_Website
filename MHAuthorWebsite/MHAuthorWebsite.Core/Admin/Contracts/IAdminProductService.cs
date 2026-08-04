using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin.Product;
using AddProductDto = MHAuthorWebsite.Core.Admin.Dto.AddProductDto;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminProductService : IProductService
{
    Task<ServiceResult> AddProductAsync(AddProductDto model);

    Task<ServiceResult<EditProductDto>> GetProductForEditAsync(Guid productId);

    Task<ServiceResult> UpdateProductAsync(EditProductDto model);

    Task<ServiceResult> DeleteProductAsync(Guid productId);

    Task<ICollection<ProductTypeAttributesDto>> GetProductTypeAttributesAsync(int productTypeId);

    Task<ICollection<ProductListItemDto>> GetProductsListReadonlyAsync();

    Task<ServiceResult> ToggleProductPublicityAsync(Guid productId);

    Task<ICollection<Guid>> GetImageIdsByProductId(Guid productId);

    Task<ServiceResult<decimal>> GetProductPriceReadonlyAsync(Guid productId);

    Task<ServiceResult> AddDiscountAsync(AddProductDiscountDto model);

    Task<ServiceResult> EndDiscountAsync(Guid productId);
}