using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using Microsoft.AspNetCore.Http;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;

namespace MHAuthorWebsite.Core.Admin;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageService(Cloudinary cloudinary) => _cloudinary = cloudinary;

    public async Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<IFormFile> images)
    {
        if (images.Count == 0 || images.Any(i => i.Length == 0))
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
            bool isThumbnail = i == 0; // First image is the thumbnail

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
                IsThumbnail = isThumbnail
            });
        }

        return ServiceResult<ICollection<ImageUploadResultDto>>.Ok(results.ToArray());
    }

    public async Task DeleteImageAsync(string imagePath)
    {
        throw new NotImplementedException("DeleteImageAsync method is not implemented yet."); // TODO
    }
}