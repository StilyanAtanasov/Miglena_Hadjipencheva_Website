using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Common;
using MHAuthorWebsite.Web.ViewModels.ProductType;

namespace MHAuthorWebsite.Core;

public class ProductTypeService : IProductTypeService
{
    private readonly IApplicationRepository _repository;

    public ProductTypeService(IApplicationRepository repository) =>  _repository = repository;
    

    public Task<ServiceResult> AddProductTypeAsync(AddProductTypeForm model)
    {
        throw new NotImplementedException();
    }
}
