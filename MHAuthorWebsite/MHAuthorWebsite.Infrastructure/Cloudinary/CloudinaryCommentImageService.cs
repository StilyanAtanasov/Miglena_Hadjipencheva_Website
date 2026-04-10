using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Images;
using MHAuthorWebsite.Core.Dtos.ProductComment;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;
using static MHAuthorWebsite.GCommon.ApplicationRules.ProductCommentImages;

namespace MHAuthorWebsite.Infrastructure.Cloudinary;

public class CloudinaryCommentImageService : CloudinaryImageService, ICommentImageService
{
    private readonly IImageService _imageService;
    private readonly ILogger<CloudinaryCommentImageService> _logger;

    public CloudinaryCommentImageService(
        IImageService imageService,
        ICloudinaryService cloudinaryService, 
        ILogger<CloudinaryCommentImageService> logger, 
        ILogger<CloudinaryImageService> baseLogger)
        : base(cloudinaryService, baseLogger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<ServiceResult<ICollection<ProductCommentImagesUploadDto>>> UploadCommentImagesAsync(
        ICollection<UploadImageRequestDto> images,
        CancellationToken cancellationToken = default)
    {
        if (images.Count == 0)
            return ServiceResult<ICollection<ProductCommentImagesUploadDto>>.Failure(
                new Dictionary<string, string> { { "Images", "Не са намерени изображения." } });

        if (images.Any(i => i.Content.CanSeek && i.Content.Length > MaxImageSizeBytes))
            return ServiceResult<ICollection<ProductCommentImagesUploadDto>>.Failure(
                new Dictionary<string, string> { { "Images", $"Всяко изображение трябва да е до {MaxImageSizeMb} MB." } });

        ICollection<UploadImageRequestDto> originals = new List<UploadImageRequestDto>();
        ICollection<UploadImageRequestDto> copies = new List<UploadImageRequestDto>();

        List<Stream> streamsToDispose = new();

        foreach (UploadImageRequestDto file in images)
        {
            if (file.Content.CanSeek) file.Content.Position = 0;

            using var bufferStream = new MemoryStream();
            await file.Content.CopyToAsync(bufferStream, cancellationToken);
            byte[] buffer = bufferStream.ToArray();

            var originalStream = new MemoryStream(buffer);
            var copyStream = new MemoryStream(buffer);

            streamsToDispose.Add(originalStream);
            streamsToDispose.Add(copyStream);

            originals.Add(new UploadImageRequestDto { Content = originalStream, FileName = file.FileName, ContentType = file.ContentType });
            copies.Add(new UploadImageRequestDto { Content = copyStream, FileName = file.FileName, ContentType = file.ContentType });
        }

        try
        {
            Task<ServiceResult<ICollection<ImageUploadResultDto>>> uploadImageTask =
                _imageService.UploadImagesAsync(originals, CommentImagesFolder, ImageMaxWidth, cancellationToken);
            Task<ServiceResult<ICollection<ImageUploadResultDto>>> uploadPreviewTask =
                _imageService.UploadImagesAsync(copies, CommentImagePreviewsFolder, ImagePreviewMaxWidth, cancellationToken);

            await Task.WhenAll(uploadImageTask, uploadPreviewTask);

            ServiceResult<ICollection<ImageUploadResultDto>> mainImages = uploadImageTask.Result;
            ServiceResult<ICollection<ImageUploadResultDto>> previewImages = uploadPreviewTask.Result;

            ICollection<ProductCommentImagesUploadDto> uploadResults = new HashSet<ProductCommentImagesUploadDto>();
            for (int i = 0; i < mainImages.Result!.Count; i++)
            {
                uploadResults.Add(new ProductCommentImagesUploadDto
                {
                    Image = mainImages.Result.ElementAt(i),
                    Preview = previewImages.Result!.ElementAt(i)
                });
            }

            _logger.LogInformation("Successfully uploaded {Count} comment images.", uploadResults.Count);
            return ServiceResult<ICollection<ProductCommentImagesUploadDto>>.Ok(uploadResults);
        }
        finally
        {
            foreach (var stream in streamsToDispose) await stream.DisposeAsync();
        }
    }

    public async Task<ServiceResult> DeleteCommentImagesAsync(ICollection<string> publicIds, CancellationToken cancellationToken = default)
    {
        if (publicIds.Count == 0)
            return ServiceResult.Failure(new() { ["Count"] = "No image identifiers provided for deletion." });

        ICollection<Task<ServiceResult>> deleteTasks = new List<Task<ServiceResult>>();

        foreach (string publicId in publicIds) deleteTasks.Add(_imageService.DeleteImageAsync(publicId, cancellationToken));

        await Task.WhenAll(deleteTasks);

        bool allSucceeded = deleteTasks.All(t => t.Result.Success);
        if (allSucceeded)
        {
            _logger.LogInformation("Successfully deleted {Count} comment images.", deleteTasks.Count);
            return ServiceResult.Ok();
        }

        string[] failed = deleteTasks
            .Where(t => !t.Result.Success)
            .SelectMany(t => t.Result.Errors.Select(e => e.Value))
            .ToArray();

        return ServiceResult.Failure(new() { ["Images"] = $"Some images could not be deleted: {string.Join(", ", failed)}" });
    }
}
