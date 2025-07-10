using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.ProductType;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductTypeService
{
    Task<ServiceResult> AddProductTypeAsync(AddProductTypeForm model);
}
