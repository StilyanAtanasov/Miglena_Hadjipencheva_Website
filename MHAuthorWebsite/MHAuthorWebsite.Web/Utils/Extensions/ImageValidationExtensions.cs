using static MHAuthorWebsite.GCommon.ApplicationRules.Cloudinary;

namespace MHAuthorWebsite.Web.Utils.Extensions;

public static class ImageValidationExtensions
{
    public static bool ExceedsCloudinarySizeLimit(this IFormFile file)
        => file.Length > MaxImageSizeBytes;

    public static bool ContainsImageExceedingCloudinarySizeLimit(this IEnumerable<IFormFile> files)
        => files.Any(file => file.ExceedsCloudinarySizeLimit());

    public static string GetCloudinarySizeLimitValidationMessage()
        => $"Всяко изображение трябва да е до {MaxImageSizeMb} MB.";
}
