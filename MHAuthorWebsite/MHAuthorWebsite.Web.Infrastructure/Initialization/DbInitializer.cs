using MHAuthorWebsite.Core.Admin.Dto;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;
using static MHAuthorWebsite.GCommon.ApplicationRules.CommentImages;

namespace MHAuthorWebsite.Web.Infrastructure.Initialization;

public static class DbInitializer
{
    public static async Task GenerateCommentImagesPreviewsAsync(ApplicationDbContext db, IImageService imageService)
    {
        ProductCommentImage[] commentImages = await db.ProductCommentsImages
            .Where(c => c.PreviewUrl == string.Empty)
            .ToArrayAsync();

        if (commentImages.Length == 0) return;

        ServiceResult<ICollection<ImageUploadResultDto>> sr = await imageService
            .UploadImagesAsync(commentImages
                .Select(ci => ci.ImageUrl)
                .ToArray(), CommentImagePreviewsFolder, ImagePreviewMaxWidth);

        if (sr.Success)
        {
            for (int i = 0; i < commentImages.Length; i++)
            {
                commentImages[i].PreviewUrl = sr.Result!.ElementAt(i).ImageUrl;
                commentImages[i].PreviewPublicId = sr.Result!.ElementAt(i).PublicId;
            }
        }

        await db.SaveChangesAsync();
    }
}