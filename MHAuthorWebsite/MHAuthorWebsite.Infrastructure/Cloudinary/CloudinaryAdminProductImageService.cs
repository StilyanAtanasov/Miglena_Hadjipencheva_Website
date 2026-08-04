using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Images;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductImage;

namespace MHAuthorWebsite.Infrastructure.Cloudinary;

public class CloudinaryAdminProductImageService : CloudinaryImageService, IAdminProductImageService
{
    private readonly IApplicationRepository _repository;
    private readonly IImageService _imageService;
    private readonly ICloudinaryAdminProductImageDataService _dataService;
    private readonly ILogger<CloudinaryAdminProductImageService> _logger;

    public CloudinaryAdminProductImageService(
        IApplicationRepository repository, 
        ICloudinaryAdminProductImageDataService dataService, 
        IImageService imageService, 
        ICloudinaryService cloudinaryService, 
        ILogger<CloudinaryAdminProductImageService> logger, 
        ILogger<CloudinaryImageService> baseLogger)
        : base(cloudinaryService, baseLogger)
    {
        _repository = repository;
        _imageService = imageService;
        _dataService = dataService;
        _logger = logger;
    }

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductImagesAsync(ICollection<UploadImageRequestDto> images, CancellationToken cancellationToken = default)
         => UploadImagesAsync(images, ImageFolder, OriginalWidth, cancellationToken);

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductImagesAsync(ICollection<string> imageUrls, CancellationToken cancellationToken = default)
        => UploadImagesAsync(imageUrls, ImageFolder, OriginalWidth, cancellationToken);

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductThumbnailAsync(UploadImageRequestDto image, CancellationToken cancellationToken = default)
        => UploadImagesAsync(new[] { image }, ThumbnailFolder, ThumbnailWidth, cancellationToken);

    public Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadProductThumbnailAsync(string imageUrl, CancellationToken cancellationToken = default)
        => UploadImagesAsync(new[] { imageUrl }, ThumbnailFolder, ThumbnailWidth, cancellationToken);

    public async Task<ServiceResult<Guid?>> LinkImagesToProductAsync(ICollection<UploadImageRequestDto> images, int? titleImageIndex, Guid productId, CancellationToken cancellationToken = default)
    {
        if (images.Count == 0 || images.Any(i => i.Content.CanSeek && i.Content.Length == 0)
            || titleImageIndex > images.Count - 1 || titleImageIndex < 0)
            return ServiceResult<Guid?>.Failure();

        Product? product = await _dataService.GetNonDeletedProductByIdAsync(productId, cancellationToken);

        if (product is null) return ServiceResult<Guid?>.Failure();

        ProductImage? titleImage = null;
        ServiceResult<ICollection<ImageUploadResultDto>> sr = await UploadProductImagesAsync(images, cancellationToken);

        for (int i = 0; i < sr.Result!.Count; i++)
        {
            ImageUploadResultDto image = sr.Result.ElementAt(i);
            ProductImage dbImage = new()
            {
                ProductId = productId,
                AltText = product.Name,
                ImageUrl = image.ImageUrl,
                PublicId = image.PublicId,
            };

            if (titleImageIndex == i) titleImage = dbImage;

            await _repository.AddAsync(dbImage);
        }

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Linked {Count} images to product {ProductId}.", sr.Result!.Count, productId);

        return ServiceResult<Guid?>.Ok(titleImage?.Id);
    }

    public async Task<ServiceResult> DeleteProductImageByIdAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        ProductImage? image = await _dataService.GetProductImageByIdAsync(imageId, cancellationToken);

        if (image is null) return ServiceResult.NotFound();

        _repository.Delete(image);
        await _repository.SaveChangesAsync();

        ServiceResult deleteResult = await _imageService.DeleteImageAsync(image.PublicId, cancellationToken);

        _logger.LogInformation("Deleted product image {ImageId}.", imageId);
        return !deleteResult.Success ? ServiceResult.Failure() : ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateProductTitleImageAsync(Guid productId, Guid newTitleImageId, CancellationToken cancellationToken = default)
    {
        Product product = await _dataService.GetNonDeletedProductForTitleImageUpdateByIdAsync(productId, cancellationToken);

        if (product.Thumbnail.Image.Id == newTitleImageId) return ServiceResult.Ok();

        ProductImage? newTitleImage =
            await _dataService.GetProductImageForTitleImageUpdateByIdAsync(productId, newTitleImageId, cancellationToken);

        if (newTitleImage is null) return ServiceResult.Failure();

        ServiceResult<ICollection<ImageUploadResultDto>> sr = await UploadProductThumbnailAsync(newTitleImage.ImageUrl, cancellationToken);
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

        ServiceResult r = await _imageService.DeleteImageAsync(oldPublicId, cancellationToken);
        if (!r.Success) return ServiceResult.Failure();

        await _repository.SaveChangesAsync();

        _logger.LogInformation("Updated title image for product {ProductId} to {NewTitleImageId}.", productId, newTitleImageId);

        return ServiceResult.Ok();
    }
}
