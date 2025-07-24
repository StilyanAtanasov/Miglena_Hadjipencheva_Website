namespace MHAuthorWebsite.GCommon;

public static class ApplicationRules
{
    public static class Application
    {
        public const string ProjectName = "MHAuthorWebsite";
        public const string WebsiteName = "Миглена Хаджипенчева"; // Use this for Layout and branding
    }

    public static class Pagination
    {
        public const byte PageSize = 10;
    }

    public static class Product
    {
        public const byte MaxImages = 10;
    }

    public static class SortTypes
    {
        public const string Recommended = "recommended";
        public const string PriceDesc = "price_desc";
        public const string PriceAsc = "price_asc";
        public const string Likes = "likes";

        public static readonly Dictionary<string, string> DisplayNames = new()
        {
            [Recommended] = "Предложени",
            [PriceDesc] = "Цена: низходящо",
            [PriceAsc] = "Цена: възходящо",
            [Likes] = "Харесвания"
        };
    }

    public static class Cloudinary
    {
        public const string ImageFolder = $"{Application.ProjectName}/products/originals";
        public const string ThumbnailFolder = $"{Application.ProjectName}/products/thumbnails";
    }
}