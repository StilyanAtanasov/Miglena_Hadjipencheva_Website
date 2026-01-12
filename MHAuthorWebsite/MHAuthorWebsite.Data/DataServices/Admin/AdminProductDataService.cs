using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Admin.Product;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices.Admin;

public class AdminProductDataService : IAdminProductDataService
{
    private readonly IApplicationRepository _repository;

    public AdminProductDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<ICollection<Guid>> GetImageIdsByProductId(Guid productId)
    => await _repository
        .WhereReadonly<ProductImage>(i => i.ProductId == productId)
        .IgnoreQueryFilters()
        .Select(i => i.Id)
        .ToArrayAsync();

    public async Task<Product?> GetNonDeletedProductByIdAsync(Guid productId)
    => await _repository
        .All<Product>()
        .IgnoreQueryFilters()
        .Where(p => !p.IsDeleted)
        .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<Product?> GetProductForUpdateByIdAsync(Guid productId)
    => await _repository
        .All<Product>()
        .IgnoreQueryFilters()
        .Where(p => !p.IsDeleted)
        .Include(p => p.Attributes)
        .Include(p => p.ProductType)
        .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<Product?> GetProductForEditByIdReadonlyAsync(Guid productId)
    => await _repository
        .AllReadonly<Product>()
        .IgnoreQueryFilters()
        .Where(p => !p.IsDeleted)
        .Include(p => p.Attributes)
            .ThenInclude(a => a.AttributeDefinition)
        .Include(p => p.ProductType)
        .Include(p => p.Images)
        .Include(p => p.Thumbnail)
        .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<ICollection<ProductListItemDto>> GetProductsListReadonlyAsync()
    => await _repository
        .AllReadonly<Product>()
        .IgnoreQueryFilters()
        .Include(p => p.ProductType)
        .Include(p => p.Discounts)
        .Where(p => !p.IsDeleted)
        .Select(p => new ProductListItemDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ProductTypeName = p.ProductType.Name,
            IsPublic = p.IsPublic,
            HasActiveDiscount = p.Discounts.Any(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
        })
        .ToArrayAsync();
}