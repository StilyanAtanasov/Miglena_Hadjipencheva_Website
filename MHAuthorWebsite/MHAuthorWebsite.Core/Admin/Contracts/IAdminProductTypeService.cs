using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.ProductType;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminProductTypeService
{
    Task<ServiceResult> AddProductTypeAsync(AddProductTypeDto model);

    Task<ICollection<ProductTypeDto>> GetAllReadonlyAsync();
}