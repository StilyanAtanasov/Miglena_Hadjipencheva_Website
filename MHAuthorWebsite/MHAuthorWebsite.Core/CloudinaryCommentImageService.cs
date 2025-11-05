using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using Microsoft.AspNetCore.Http;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;
using static MHAuthorWebsite.GCommon.ApplicationRules.ProductCommentImages;

namespace MHAuthorWebsite.Core;

public class CloudinaryCommentImageService : CloudinaryImageService, ICommentImageService
{
    private readonly IImageService _imageService;

    public CloudinaryCommentImageService(IImageService imageService,
        ICloudinaryService cloudinaryService)
        : base(cloudinaryService)
        => _imageService = imageService;

    public async Task<ServiceResult<ICollection<ProductCommentImagesUploadDto>>> UploadCommentImagesAsync(
        ICollection<IFormFile> images)
    {
        ICollection<IFormFile> copies = new List<IFormFile>();
        foreach (IFormFile file in images)
        {
            MemoryStream ms = new();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            copies.Add(new FormFile(ms, 0, ms.Length, file.Name, file.FileName)
            {
                Headers = file.Headers,
                ContentType = file.ContentType
            });
        }

        Task<ServiceResult<ICollection<ImageUploadResultDto>>> uploadImageTask =
            _imageService.UploadImagesAsync(images, CommentImagesFolder, ImageMaxWidth);
        Task<ServiceResult<ICollection<ImageUploadResultDto>>> uploadPreviewTask =
            _imageService.UploadImagesAsync(copies, CommentImagePreviewsFolder, ImagePreviewMaxWidth);

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

        return ServiceResult<ICollection<ProductCommentImagesUploadDto>>.Ok(uploadResults);
    }

    public async Task<ServiceResult> DeleteCommentImagesAsync(ICollection<string> publicIds)
    {
        if (publicIds.Count == 0)
            return ServiceResult.Failure(new() { ["Count"] = "No image identifiers provided for deletion." });

        ICollection<Task<ServiceResult>> deleteTasks = new List<Task<ServiceResult>>();

        foreach (string publicId in publicIds) deleteTasks.Add(_imageService.DeleteImageAsync(publicId));

        await Task.WhenAll(deleteTasks);

        bool allSucceeded = deleteTasks.All(t => t.Result.Success);
        if (allSucceeded) return ServiceResult.Ok();

        string[] failed = deleteTasks
            .Where(t => !t.Result.Success)
            .SelectMany(t => t.Result.Errors.Select(e => e.Value))
            .ToArray();

        return ServiceResult.Failure(new() { ["Images"] = $"Some images could not be deleted: {string.Join(", ", failed)}" });
    }
}