using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.ProductType;

namespace MHAuthorWebsite.Core;

public class ProductTypeService : IProductTypeService
{
    private readonly IApplicationRepository _repository;

    public ProductTypeService(IApplicationRepository repository) => _repository = repository;


    public async Task<ServiceResult> AddProductTypeAsync(AddProductTypeForm model)
    {
        try
        {
            ProductType pt = new() { Name = model.Name };

            await _repository.AddAsync(pt);
            await _repository.SaveChangesAsync();

            if (model.HasAdditionalProperties)
            {
                foreach (AttributeDefinitionForm attribute in model.Attributes)
                {
                    await _repository.AddAsync<ProductAttributeDefinition>(new()
                    {
                        Key = attribute.Key,
                        Label = attribute.Label,
                        DataType = (AttributeDataType)attribute.DataType,
                        HasPredefinedValue = attribute.HasPredefinedValue,
                        IsRequired = attribute.IsRequired,
                        ProductTypeId = pt.Id
                    });
                }

                await _repository.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }

        return ServiceResult.Ok();
    }
}
