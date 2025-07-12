using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Product;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Core;

public class ProductService : IProductService
{
    private readonly IApplicationRepository _repository;

    public ProductService(IApplicationRepository repository) => _repository = repository;

    public Task<ServiceResult> AddProductAsync(AddProductForm model)
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<ProductTypeAttributesDto>> GetProductTypeAttributesAsync(int productTypeId) =>
        await _repository
            .Where<ProductAttributeDefinition>(pad => pad.ProductTypeId == productTypeId)
            .Select(pad => new ProductTypeAttributesDto
            {
                Key = pad.Key,
                Label = pad.Label,
                DataType = (int)pad.DataType,
                HasPredefinedValue = pad.HasPredefinedValue,
                IsRequired = pad.IsRequired
            })
            .ToArrayAsync();
}