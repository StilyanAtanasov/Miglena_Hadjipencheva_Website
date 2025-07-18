using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.Product;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminProductService : IProductService
{
    Task<ServiceResult> AddProductAsync(AddProductForm model);

    // Task<ServiceResult> UpdateProductAsync(UpdateProductForm model);

    Task<ServiceResult> DeleteProductAsync(Guid productId);
}