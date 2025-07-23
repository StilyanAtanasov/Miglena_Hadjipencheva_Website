using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.ProductType;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminProductTypeService
{
    Task<ServiceResult> AddProductTypeAsync(AddProductTypeForm model);

    Task<ICollection<ProductTypeDto>> GetAllReadonlyAsync();
}