using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using Microsoft.AspNetCore.Http;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;

namespace MHAuthorWebsite.Core.Admin;

public class CloudinaryImageService : IImageService
{
    private readonly ICloudinaryService _cloudinaryService;

    public CloudinaryImageService(ICloudinaryService cloudinaryService) => _cloudinaryService = cloudinaryService;

    public async Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<IFormFile> images, string folder, short width)
    {
        if (images.Count == 0 || images.Any(i => i.Length == 0))
            return ServiceResult<ICollection<ImageUploadResultDto>>.Failure();

        IEnumerable<Task<ImageUploadResult>> uploadTasks = images.Select(image =>
        {
            string fileName = Path.GetFileNameWithoutExtension(image.FileName);
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            ImageUploadParams fullUploadParams = new()
            {
                File = new FileDescription(image.FileName, image.OpenReadStream()),
                Folder = folder,
                PublicId = $"{fileName}_{timestamp}",
                Format = "avif",
                Type = "private",
                Transformation = new Transformation()
                    .Width(width)
                    .Crop("limit")
                    .FetchFormat("avif")
            };

            return _cloudinaryService.UploadAsync(fullUploadParams);
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

    public async Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImagesAsync(ICollection<string> imageUrls, string folder, short width)
    {
        if (imageUrls.Count == 0 || imageUrls.Any(i => i.Length == 0))
            return ServiceResult<ICollection<ImageUploadResultDto>>.Failure();

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

            return _cloudinaryService.UploadAsync(fullUploadParams);
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

    // TODO Use better approach fro abstraction
    public async Task<ServiceResult<ICollection<ProductImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<IFormFile> images, int titleImageId)
    {
        if (images.Count == 0 || images.Any(i => i.Length == 0) || titleImageId > images.Count - 1 || titleImageId < 0)
            return ServiceResult<ICollection<ProductImageUploadResultDto>>.Failure();

        List<ProductImageUploadResultDto> results = new();

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

            ImageUploadResult fullUpload = await _cloudinaryService.UploadAsync(fullUploadParams);

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

                previewUpload = await _cloudinaryService.UploadAsync(previewUploadParams);
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

    public async Task<ServiceResult> DeleteImageAsync(string publicId)
    {
        DeletionParams deletionParams = new(publicId)
        {
            Type = "private"
        };

        DeletionResult result = await _cloudinaryService.DestroyAsync(deletionParams);

        if (result.Result != "ok") return ServiceResult.Failure();

        return ServiceResult.Ok();
    }
}