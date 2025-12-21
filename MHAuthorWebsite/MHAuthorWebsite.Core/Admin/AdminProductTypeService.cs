using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.ProductType;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Admin;

public class AdminProductTypeService : IAdminProductTypeService
{
    private readonly IApplicationRepository _repository;

    public AdminProductTypeService(IApplicationRepository repository) => _repository = repository;

    public async Task<ServiceResult> AddProductTypeAsync(AddProductTypeDto model)
    {
        try
        {
            ProductType pt = new() { Name = model.Name };

            await _repository.AddAsync(pt);
            await _repository.SaveChangesAsync();

            if (model is { HasAdditionalProperties: true, Attributes.Count: > 0 })
            {
                foreach (AttributeDefinitionDto attribute in model.Attributes)
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

    public async Task<ICollection<ProductTypeDto>> GetAllReadonlyAsync() =>
        await _repository
            .AllReadonly<ProductType>()
            .Select(pt => new ProductTypeDto
            {
                Id = pt.Id,
                Name = pt.Name
            })
            .ToArrayAsync();
}