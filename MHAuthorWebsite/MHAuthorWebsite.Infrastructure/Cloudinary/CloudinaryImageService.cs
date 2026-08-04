using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Images;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;

namespace MHAuthorWebsite.Infrastructure.Cloudinary;

public class CloudinaryImageService : IImageService
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<CloudinaryImageService> _logger;

    public CloudinaryImageService(ICloudinaryService cloudinaryService, ILogger<CloudinaryImageService> logger)
    {
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    public async Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<UploadImageRequestDto> images, string folder, short width, CancellationToken cancellationToken = default)
    {
        if (images.Count == 0)
            return ServiceResult<ICollection<ImageUploadResultDto>>.Failure(new Dictionary<string, string> { { "Images", "Не са намерени изображения." } });

        if (images.Any(i => i.Content.CanSeek && i.Content.Length > MaxImageSizeBytes))
            return ServiceResult<ICollection<ImageUploadResultDto>>.Failure(new Dictionary<string, string>
            {
                { "Images", $"Всяко изображение трябва да е до {MaxImageSizeMb} MB." }
            });

        _logger.LogInformation("Uploading {Count} images to folder {Folder}.", images.Count, folder);

        IEnumerable<Task<ImageUploadResult>> uploadTasks = images.Select(async image =>
        {
            if (image.Content.CanSeek) image.Content.Position = 0;

            string fileName = Path.GetFileNameWithoutExtension(image.FileName);
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            ImageUploadParams fullUploadParams = new()
            {
                File = new FileDescription(image.FileName, image.Content),
                Folder = folder,
                PublicId = $"{fileName}_{timestamp}",
                Format = "avif",
                Type = "private",
                Transformation = new Transformation()
                    .Width(width)
                    .Crop("limit")
                    .FetchFormat("avif")
            };

            return await _cloudinaryService.UploadAsync(fullUploadParams, cancellationToken);
        });

        ImageUploadResult[] fullUploads = await Task.WhenAll(uploadTasks);

        return ServiceResult<ICollection<ImageUploadResultDto>>.Ok(fullUploads
            .Select(fullUpload => new ImageUploadResultDto
            {
                ImageUrl = fullUpload.SecureUrl.AbsoluteUri,
                PublicId = fullUpload.PublicId
            })
            .ToArray());
    }

    public async Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<string> imageUrls, string folder, short width, CancellationToken cancellationToken = default)
    {
        if (imageUrls.Count == 0 || imageUrls.Any(i => i.Length == 0))
            return ServiceResult<ICollection<ImageUploadResultDto>>.Failure();

        _logger.LogInformation("Uploading {Count} images from URLs to folder {Folder}.", imageUrls.Count, folder);

        IEnumerable<Task<ImageUploadResult>> uploadTasks = imageUrls.Select(imageUrl =>
        {
            string fileName = Path.GetFileNameWithoutExtension(new Uri(imageUrl).AbsolutePath);
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            ImageUploadParams fullUploadParams = new()
            {
                File = new FileDescription(imageUrl),
                Folder = folder,
                PublicId = $"{fileName}_{timestamp}",
                Format = "avif",
                Type = "private",
                Transformation = new Transformation()
                    .Width(width)
                    .Crop("limit")
                    .FetchFormat("avif")
            };

            return _cloudinaryService.UploadAsync(fullUploadParams, cancellationToken);
        });

        ImageUploadResult[] fullUploads = await Task.WhenAll(uploadTasks);

        return ServiceResult<ICollection<ImageUploadResultDto>>.Ok(fullUploads
            .Select(fullUpload => new ImageUploadResultDto
            {
                ImageUrl = fullUpload.SecureUrl.AbsoluteUri,
                PublicId = fullUpload.PublicId
            })
            .ToArray());
    }

    public async Task<ServiceResult<ICollection<ProductImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<UploadImageRequestDto> images, int titleImageId, CancellationToken cancellationToken = default)
    {
        if (images.Count == 0 || images.Any(i => i.Content.CanSeek && i.Content.Length == 0) || titleImageId > images.Count - 1 || titleImageId < 0)
            return ServiceResult<ICollection<ProductImageUploadResultDto>>.Failure();

        _logger.LogInformation("Uploading {Count} product images with preview.", images.Count);

        List<ProductImageUploadResultDto> results = new();

        for (int i = 0; i < images.Count; i++)
        {
            UploadImageRequestDto image = images.ElementAt(i);

            await using Stream input = image.Content;
            using MemoryStream fullStream = new();
            using MemoryStream previewStream = new();

            await input.CopyToAsync(fullStream, cancellationToken);
            fullStream.Position = 0;
            previewStream.Write(fullStream.ToArray());
            previewStream.Position = 0;

            string fileName = Path.GetFileNameWithoutExtension(image.FileName);
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            bool isThumbnail = i == titleImageId;

            ImageUploadParams fullUploadParams = new()
            {
                File = new FileDescription(image.FileName, fullStream),
                Folder = ImageFolder,
                PublicId = $"{fileName}_{timestamp}",
                Format = "avif",
                Type = "private",
                Transformation = new Transformation()
                    .Width(1200)
                    .Crop("limit")
                    .FetchFormat("avif")
            };

            ImageUploadResult fullUpload = await _cloudinaryService.UploadAsync(fullUploadParams, cancellationToken);

            ImageUploadResult? previewUpload = null;
            if (isThumbnail)
            {
                ImageUploadParams previewUploadParams = new()
                {
                    File = new FileDescription(image.FileName, previewStream),
                    Folder = ThumbnailFolder,
                    PublicId = $"{fileName}_thumb_{timestamp}",
                    Format = "avif",
                    Type = "private",
                    Transformation = new Transformation()
                        .Width(250)
                        .Crop("scale")
                        .FetchFormat("avif")
                };

                previewUpload = await _cloudinaryService.UploadAsync(previewUploadParams, cancellationToken);
            }

            results.Add(new ProductImageUploadResultDto
            {
                OriginalUrl = fullUpload.SecureUrl.AbsoluteUri,
                PreviewUrl = previewUpload?.SecureUrl.AbsoluteUri ?? null,
                PublicId = fullUpload.PublicId,
                ThumbnailPublicId = previewUpload?.PublicId ?? null,
                IsThumbnail = isThumbnail
            });
        }

        return ServiceResult<ICollection<ProductImageUploadResultDto>>.Ok(results.ToArray());
    }

    public async Task<ServiceResult> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        DeletionParams deletionParams = new(publicId)
        {
            Type = "private"
        };

        DeletionResult result = await _cloudinaryService.DestroyAsync(deletionParams, cancellationToken);

        if (result.Result != "ok") return ServiceResult.Failure();

        _logger.LogInformation("Deleting Cloudinary image {PublicId}.", publicId);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteImagesAsync(ICollection<string> publicIds, CancellationToken cancellationToken = default)
    {
        if (publicIds.Count == 0) return ServiceResult.Failure();

        IEnumerable<Task<ServiceResult>> deletionTasks = publicIds.Select(id => DeleteImageAsync(id, cancellationToken));
        ServiceResult[] results = await Task.WhenAll(deletionTasks);

        if (results.Any(result => !result.Success))
            return ServiceResult.Failure();

        _logger.LogInformation("Deleting {Count} Cloudinary images.", publicIds.Count);

        return ServiceResult.Ok();
    }
}
