using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Web.ViewModels.Product;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminProductService : IProductService
{
    Task<ServiceResult> AddProductAsync(AddProductForm model);

    Task<ServiceResult<EditProductFormViewModel>> GetProductForEditAsync(Guid productId);

    Task<ServiceResult> UpdateProductAsync(EditProductFormViewModel model);

    Task<ServiceResult> DeleteProductAsync(Guid productId);

    Task<ICollection<ProductTypeAttributesDto>> GetProductTypeAttributesAsync(int productTypeId);
}