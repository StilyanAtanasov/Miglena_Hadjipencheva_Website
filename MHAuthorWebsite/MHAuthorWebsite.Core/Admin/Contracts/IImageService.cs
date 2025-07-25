﻿using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using Microsoft.AspNetCore.Http;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IImageService
{
    /// <summary>
    ///     Uploads an image to the specified path.
    /// </summary>
    /// <param name="images">The image files to upload.</param>
    /// <returns>The URL of the uploaded image.</returns>
    Task<ServiceResult<ICollection<ImageUploadResultDto>>> UploadImageWithPreviewAsync(ICollection<IFormFile> images, int titleImageId);

    /// <summary>
    ///     Deletes an image from the specified path.
    /// </summary>
    /// <param name="imagePath">The path of the image to delete.</param>
    Task DeleteImageAsync(string imagePath);
}