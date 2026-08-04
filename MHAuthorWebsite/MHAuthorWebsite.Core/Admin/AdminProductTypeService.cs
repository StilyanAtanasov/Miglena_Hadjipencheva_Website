using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.ProductType;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.Extensions.Logging;

namespace MHAuthorWebsite.Core.Admin;

public class AdminProductTypeService : IAdminProductTypeService
{
    private readonly IApplicationRepository _repository;
    private readonly ILogger<AdminProductTypeService> _logger;

    public AdminProductTypeService(IApplicationRepository repository, ILogger<AdminProductTypeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ServiceResult> AddProductTypeAsync(AddProductTypeDto model)
    {
        try
        {
            ProductType pt = new() { Name = model.Name };

            if (model is { HasAdditionalProperties: true, Attributes.Count: > 0 })
            {
                foreach (AttributeDefinitionDto attribute in model.Attributes)
                {
                    ProductAttributeDefinition attrDefinition = new()
                    {
                        Key = attribute.Key,
                        Label = attribute.Label,
                        DataType = (AttributeDataType)attribute.DataType,
                        IsRequired = attribute.IsRequired
                    };

                    if (attribute is { DataType: (int)AttributeDataType.Dropdown, PredefinedValues.Count: > 0 })
                        foreach (string value in attribute.PredefinedValues)
                            attrDefinition.ProductAttributeOptions.Add(new()
                            {
                                Value = value,
                                AttributeDefinitionId = attrDefinition.Id
                            });

                    pt.AttributeDefinitions.Add(attrDefinition);
                }
            }

            await _repository.AddAsync(pt);
            await _repository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return ServiceResult.Failure();
        }

        _logger.LogInformation("Admin added new product type: {Name}", model.Name);
        return ServiceResult.Ok();
    }

    public async Task<ICollection<ProductTypeDto>> GetAllReadonlyAsync()
    {
        var result = await _repository
            .AllReadonly<ProductType>()
            .Select(pt => new ProductTypeDto
            {
                Id = pt.Id,
                Name = pt.Name
            })
            .ToArrayAsync();

        _logger.LogInformation("Admin retrieved all product types.");

        return result;
    }
}