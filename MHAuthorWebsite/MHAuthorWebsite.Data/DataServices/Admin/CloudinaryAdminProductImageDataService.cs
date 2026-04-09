using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices.Admin;

public class CloudinaryAdminProductImageDataService : ICloudinaryAdminProductImageDataService
{
    private readonly IApplicationRepository _repository;

    public CloudinaryAdminProductImageDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<Product?> GetNonDeletedProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
   => await _repository
       .All<Product>()
       .IgnoreQueryFilters()
       .Where(p => p.Id == productId && !p.IsDeleted)
       .FirstOrDefaultAsync(cancellationToken);

    public async Task<Product> GetNonDeletedProductForTitleImageUpdateByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    => await _repository
        .All<Product>()
        .Include(p => p.Thumbnail)
        .ThenInclude(t => t.Image)
        .IgnoreQueryFilters()
        .Where(p => p.Id == productId && !p.IsDeleted)
        .FirstAsync(cancellationToken);

    public async Task<ProductImage?> GetProductImageByIdAsync(Guid imageId, CancellationToken cancellationToken = default)
    => await _repository
        .All<ProductImage>()
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken);

    public async Task<ProductImage?> GetProductImageForTitleImageUpdateByIdAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    => await _repository
        .All<ProductImage>()
        .IgnoreQueryFilters()
        .Include(i => i.Product)
        .Where(i => !i.Product.IsDeleted)
        .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == imageId, cancellationToken);
}
