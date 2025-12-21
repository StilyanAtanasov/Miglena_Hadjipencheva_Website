using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices.Admin;

public class CloudinaryAdminProductImageDataService : ICloudinaryAdminProductImageDataService
{
    private readonly IApplicationRepository _repository;

    public CloudinaryAdminProductImageDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Product?> GetNonDeletedProductByIdAsync(Guid productId)
   => await _repository
       .All<Product>()
       .IgnoreQueryFilters()
       .Where(p => p.Id == productId && !p.IsDeleted)
       .FirstOrDefaultAsync();

    public async Task<Product> GetNonDeletedProductForTitleImageUpdateByIdAsync(Guid productId)
    => await _repository
        .All<Product>()
        .Include(p => p.Thumbnail)
        .ThenInclude(t => t.Image)
        .IgnoreQueryFilters()
        .Where(p => p.Id == productId && !p.IsDeleted)
        .FirstAsync();

    public async Task<ProductImage?> GetProductImageByIdAsync(Guid imageId)
    => await _repository
        .All<ProductImage>()
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(i => i.Id == imageId);

    public async Task<ProductImage?> GetProductImageForTitleImageUpdateByIdAsync(Guid productId, Guid imageId)
    => await _repository
        .All<ProductImage>()
        .IgnoreQueryFilters()
        .Include(i => i.Product)
        .Where(i => !i.Product.IsDeleted)
        .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == imageId);
}