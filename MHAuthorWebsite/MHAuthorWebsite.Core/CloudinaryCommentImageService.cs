using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dto;
using Microsoft.AspNetCore.Http;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;
using static MHAuthorWebsite.GCommon.ApplicationRules.CommentImages;

namespace MHAuthorWebsite.Core;

public class CloudinaryCommentImageService : CloudinaryImageService, ICommentImageService
{
    private readonly IImageService _imageService;

    public CloudinaryCommentImageService(IImageService imageService,
        ICloudinaryService cloudinaryService)
        : base(cloudinaryService)
        => _imageService = imageService;

    public async Task<ServiceResult<ICollection<ProductCommentImagesUploadDto>>> UploadCommentImagesAsync(
        ICollection<string> imageUrls)
    {
        Task<ServiceResult<ICollection<ImageUploadResultDto>>> uploadImageTask =
            _imageService.UploadImagesAsync(imageUrls, CommentImagesFolder, ImageMaxWidth);
        Task<ServiceResult<ICollection<ImageUploadResultDto>>> uploadPreviewTask =
            _imageService.UploadImagesAsync(imageUrls, CommentImagePreviewsFolder, ImagePreviewMaxWidth);

        await Task.WhenAll(uploadImageTask, uploadPreviewTask);

        ServiceResult<ICollection<ImageUploadResultDto>> mainImages = uploadImageTask.Result;
        ServiceResult<ICollection<ImageUploadResultDto>> previewImages = uploadPreviewTask.Result;

        ICollection<ProductCommentImagesUploadDto> uploadResults = new ProductCommentImagesUploadDto[mainImages.Result!.Count];
        for (int i = 0; i < mainImages.Result.Count; i++)
        {
            uploadResults.Add(new ProductCommentImagesUploadDto
            {
                Image = mainImages.Result.ElementAt(i),
                Preview = previewImages.Result!.ElementAt(i)
            });
        }

        return ServiceResult<ICollection<ProductCommentImagesUploadDto>>.Ok(uploadResults);
    }

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
}