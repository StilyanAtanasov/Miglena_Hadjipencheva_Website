using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Web.ViewModels.ProductType;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHAuthorWebsite.Core.Contracts;

public interface IProductTypeService
{
    Task<ServiceResult> AddProductTypeAsync(AddProductTypeForm model);

    Task<ICollection<ProductTypeDto>> GetAllReadonlyAsync();
}
