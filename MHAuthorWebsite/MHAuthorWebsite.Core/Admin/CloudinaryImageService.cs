using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace MHAuthorWebsite.Core.Admin;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageService(Cloudinary cloudinary) => _cloudinary = cloudinary;

    public async Task<ServiceResult<string[]>> UploadImagesAsync(ICollection<IFormFile> images)
    {
        if (images.Count == 0 || images.Any(i => i.Length == 0))
            return ServiceResult<string[]>.Failure();

        string[] imageUrls = new string[images.Count];

        for (int i = 0; i < images.Count; i++)
        {
            IFormFile image = images.ElementAt(i);
            ImageUploadResult uploadResult = await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(image.FileName, image.OpenReadStream())
            });

            imageUrls[i] = uploadResult.SecureUrl.AbsoluteUri;
        }

        return ServiceResult<string[]>.Ok(imageUrls);
    }

    public async Task DeleteImageAsync(string imagePath)
    {
        throw new NotImplementedException("DeleteImageAsync method is not implemented yet."); // TODO
    }
}