using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;

namespace MHAuthorWebsite.Core.Admin;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;
    private readonly IApplicationRepository _repository;

    public CloudinaryImageService(Cloudinary cloudinary, IApplicationRepository repository)
    {
        _cloudinary = cloudinary;
        _repository = repository;
    }

    public async Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<IFormFile> images, int titleImageId)
    {
        if (images.Count == 0 || images.Any(i => i.Length == 0) || titleImageId > images.Count - 1)
            return ServiceResult<ICollection<ImageUploadResultDto>>.Failure();

        List<ImageUploadResultDto> results = new();

        for (int i = 0; i < images.Count; i++)
        {
            IFormFile image = images.ElementAt(i);

            await using Stream input = image.OpenReadStream();
            using MemoryStream fullStream = new();
            using MemoryStream previewStream = new();

            await input.CopyToAsync(fullStream);
            fullStream.Position = 0;
            previewStream.Write(fullStream.ToArray());
            previewStream.Position = 0;

            string fileName = Path.GetFileNameWithoutExtension(image.FileName);
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            bool isThumbnail = i == titleImageId;

            // FULL image - max width, AVIF, aspect preserved
            ImageUploadParams fullUploadParams = new()
            {
                File = new FileDescription(image.FileName, fullStream),
                Folder = ImageFolder,
                PublicId = $"{fileName}_{timestamp}",
                Format = "avif",
                Type = "private",
                Transformation = new Transformation()
                    .Width(1200)
                    .Crop("limit") // Resize down, preserve aspect
                    .FetchFormat("avif")
            };

            ImageUploadResult fullUpload = await _cloudinary.UploadAsync(fullUploadParams);

            ImageUploadResult? previewUpload = null;
            if (isThumbnail)
            {
                // Thumbnail image - small thumbnail, AVIF
                ImageUploadParams previewUploadParams = new()
                {
                    File = new FileDescription(image.FileName, previewStream),
                    Folder = ThumbnailFolder,
                    PublicId = $"{fileName}_thumb_{timestamp}",
                    Format = "avif",
                    Type = "private",
                    Transformation = new Transformation()
                        .Width(250)
                        .Crop("scale") // Shrink, preserve ratio
                        .FetchFormat("avif")
                };

                previewUpload = await _cloudinary.UploadAsync(previewUploadParams);
            }

            results.Add(new ImageUploadResultDto
            {
                OriginalUrl = fullUpload.SecureUrl.AbsoluteUri,
                PreviewUrl = previewUpload?.SecureUrl.AbsoluteUri ?? null,
                PublicId = fullUpload.PublicId,
                ThumbnailPublicId = previewUpload?.PublicId ?? null,
                IsThumbnail = isThumbnail
            });
        }

        return ServiceResult<ICollection<ImageUploadResultDto>>.Ok(results.ToArray());
    }

    public async Task<ServiceResult<Guid?>> LinkImagesToProductAsync(ICollection<IFormFile> images, int? titleImageIndex, Guid productId)
    {
        if (images.Count == 0 || images.Any(i => i.Length == 0) || titleImageIndex > images.Count - 1)
            return ServiceResult<Guid?>.Failure();

        Image? titleImage = null;
        for (int i = 0; i < images.Count; i++)
        {
            IFormFile image = images.ElementAt(i);

            string fileName = Path.GetFileNameWithoutExtension(image.FileName);
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            bool isThumbnail = i == titleImageIndex;

            // FULL image - max width, AVIF, aspect preserved
            ImageUploadParams fullUploadParams = new()
            {
                File = new FileDescription(image.FileName, image.OpenReadStream()),
                Folder = ImageFolder,
                PublicId = $"{fileName}_{timestamp}",
                Format = "avif",
                Type = "private",
                Transformation = new Transformation()
                    .Width(1200)
                    .Crop("limit") // Resize down, preserve aspect
                    .FetchFormat("avif")
            };

            ImageUploadResult fullUpload = await _cloudinary.UploadAsync(fullUploadParams);

            string? productName = await _repository
                .AllReadonly<Product>()
                .IgnoreQueryFilters()
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Select(p => p.Name)
                .FirstOrDefaultAsync();

            Image dbImage = new()
            {
                ProductId = productId,
                AltText = productName ?? fileName, // TODO Probably use the image title
                ImageUrl = fullUpload.SecureUrl.AbsoluteUri,
                ThumbnailUrl = null,
                PublicId = fullUpload.PublicId,
                ThumbnailPublicId = null,
                IsThumbnail = false
            };

            if (isThumbnail) titleImage = dbImage;

            await _repository.AddAsync(dbImage);
        }

        await _repository.SaveChangesAsync();
        return ServiceResult<Guid?>.Ok(titleImage?.Id);
    }

    public async Task<ServiceResult> DeleteImageAsync(string publicId)
    {
        DeletionParams deletionParams = new(publicId)
        {
            Type = "private"
        };

        DeletionResult result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Result != "ok") return ServiceResult.Failure();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteProductImageByIdAsync(Guid imageId)
    {
        Image? image = await _repository
            .All<Image>()
            .IgnoreQueryFilters()
            .Include(i => i.Product)
            .Where(i => !i.Product.IsDeleted)
            .FirstOrDefaultAsync(i => i.Id == imageId);

        if (image is null) return ServiceResult.NotFound();

        _repository.Delete(image);
        await _repository.SaveChangesAsync();

        if (image.IsThumbnail)
        {
            // If it's a thumbnail, delete the thumbnail image
            ServiceResult r = await DeleteImageAsync(image.ThumbnailPublicId!);
            if (!r.Success) return ServiceResult.Failure();
        }

        // Delete the full image
        ServiceResult deleteResult = await DeleteImageAsync(image.PublicId);
        if (!deleteResult.Success) return ServiceResult.Failure();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateProductTitleImageAsync(Guid productId, Guid newTitleImageId)
    {
        Image? currentTitleImage = await _repository
            .All<Image>()
            .IgnoreQueryFilters()
            .Include(i => i.Product)
            .Where(i => !i.Product.IsDeleted)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.IsThumbnail);

        if (currentTitleImage is not null)
        {
            if (currentTitleImage.Id == newTitleImageId) return ServiceResult.Ok();

            ServiceResult r = await DeleteImageAsync(currentTitleImage.ThumbnailPublicId!);
            if (!r.Success) return ServiceResult.Failure();

            currentTitleImage.IsThumbnail = false;
            currentTitleImage.ThumbnailUrl = null;
            currentTitleImage.ThumbnailPublicId = null;
        }

        Image? newTitleImage = await _repository
                .All<Image>()
                .IgnoreQueryFilters()
                .Include(i => i.Product)
                .Where(i => !i.Product.IsDeleted)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.Id == newTitleImageId);
        if (newTitleImage is null) return ServiceResult.Failure();

        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        ImageUploadParams previewUploadParams = new()
        {
            File = new FileDescription(newTitleImage.ImageUrl),
            Folder = ThumbnailFolder,
            PublicId = $"{newTitleImage.AltText}_thumb_{timestamp}",
            Format = "avif",
            Type = "private",
            Transformation = new Transformation()
                .Width(250)
                .Crop("scale") // Shrink, preserve ratio
                .FetchFormat("avif")
        };

        ImageUploadResult previewUpload = await _cloudinary.UploadAsync(previewUploadParams);

        newTitleImage.IsThumbnail = true;
        newTitleImage.ThumbnailUrl = previewUpload.SecureUrl.AbsoluteUri;
        newTitleImage.ThumbnailPublicId = previewUpload.PublicId;

        await _repository.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}