using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Common;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Web.ViewModels.ProductType;

namespace MHAuthorWebsite.Core;

public class ProductTypeService : IProductTypeService
{
    private readonly IApplicationRepository _repository;

    public ProductTypeService(IApplicationRepository repository) =>  _repository = repository;
    

    public async Task<ServiceResult> AddProductTypeAsync(AddProductTypeForm model)
    {
        try
        {
            await _repository.AddAsync(new ProductType
            {
                Name = model.Name
            });

            await _repository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }
       
        return ServiceResult.Ok();
    }
}
