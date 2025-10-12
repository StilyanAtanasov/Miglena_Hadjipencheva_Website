using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductImage;

namespace MHAuthorWebsite.Core.Admin;

public class CloudinaryAdminProductImageService : CloudinaryImageService, IAdminProductImageService
{
    private readonly IApplicationRepository _repository;
    private readonly IImageService _imageService;

    public CloudinaryAdminProductImageService(IApplicationRepository repository, IImageService imageService, ICloudinaryService cloudinaryService)
        : base(cloudinaryService)
    {
        _repository = repository;
        _imageService = imageService;
    }

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductImagesAsync(ICollection<IFormFile> images)
         => UploadImagesAsync(images, ImageFolder, OriginalWidth);

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductImagesAsync(ICollection<string> imageUrls)
        => UploadImagesAsync(imageUrls, ImageFolder, OriginalWidth);

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductThumbnailAsync(IFormFile image)
        => UploadImagesAsync(new[] { image }, ThumbnailFolder, ThumbnailWidth);

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductThumbnailAsync(string imageUrl)
        => UploadImagesAsync(new[] { imageUrl }, ThumbnailFolder, ThumbnailWidth);

    public async Task<ServiceResult<Guid?>> LinkImagesToProductAsync(ICollection<IFormFile> images, int? titleImageIndex, Guid productId)
    {
        if (images.Count == 0 || images.Any(i => i.Length == 0)
            || titleImageIndex > images.Count - 1 || titleImageIndex < 0)
            return ServiceResult<Guid?>.Failure();

        Product? product = await _repository
            .All<Product>()
            .IgnoreQueryFilters()
            .Where(p => p.Id == productId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (product is null) return ServiceResult<Guid?>.Failure();

        ProductImage? titleImage = null;
        ServiceResult<ICollection<ImageUploadResultDto>> sr = await UploadProductImagesAsync(images);

        for (int i = 0; i < sr.Result!.Count; i++)
        {
            ImageUploadResultDto image = sr.Result.ElementAt(i);
            ProductImage dbImage = new()
            {
                ProductId = productId,
                AltText = product.Name, // TODO Probably use the image title
                ImageUrl = image.ImageUrl,
                PublicId = image.PublicId,
            };

            if (titleImageIndex == i) titleImage = dbImage;

            await _repository.AddAsync(dbImage);
        }

        await _repository.SaveChangesAsync();
        return ServiceResult<Guid?>.Ok(titleImage?.Id);
    }

    public async Task<ServiceResult> DeleteProductImageByIdAsync(Guid imageId)
    {
        ProductImage? image = await _repository
            .All<ProductImage>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == imageId);

        if (image is null) return ServiceResult.NotFound();

        _repository.Delete(image);
        await _repository.SaveChangesAsync();

        // Delete the full image
        ServiceResult deleteResult = await _imageService.DeleteImageAsync(image.PublicId);

        return !deleteResult.Success ? ServiceResult.Failure() : ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateProductTitleImageAsync(Guid productId, Guid newTitleImageId)
    {
        Product product = await _repository
            .All<Product>()
            .Include(p => p.Thumbnail)
                .ThenInclude(t => t.Image)
            .IgnoreQueryFilters()
            .Where(p => p.Id == productId && !p.IsDeleted)
            .FirstAsync();

        if (product.Thumbnail.Image.Id == newTitleImageId) return ServiceResult.Ok();

        ProductImage? newTitleImage = await _repository
            .All<ProductImage>()
            .IgnoreQueryFilters()
            .Include(i => i.Product)
            .Where(i => !i.Product.IsDeleted)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == newTitleImageId);

        if (newTitleImage is null) return ServiceResult.Failure();

        ServiceResult<ICollection<ImageUploadResultDto>> sr = await UploadProductThumbnailAsync(newTitleImage.ImageUrl);
        if (!sr.Success) return ServiceResult.Failure();

        ProductImage oldImage = product.Thumbnail.Image;
        string oldPublicId = oldImage.PublicId;

        product.Thumbnail = new ProductThumbnail
        {
            ImageOriginalId = newTitleImage.Id,
            ProductId = productId,
            Image = new ProductImage
            {
                ProductId = productId,
                AltText = product.Name,
                ImageUrl = sr.Result!.First().ImageUrl,
                PublicId = sr.Result!.First().PublicId
            }
        };

        _repository.Delete(oldImage);

        ServiceResult r = await _imageService.DeleteImageAsync(oldPublicId);
        if (!r.Success) return ServiceResult.Failure();

        await _repository.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}