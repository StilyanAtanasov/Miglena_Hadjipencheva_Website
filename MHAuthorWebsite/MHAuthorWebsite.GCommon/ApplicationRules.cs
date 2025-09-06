namespace MHAuthorWebsite.GCommon;

public static class ApplicationRules
{
    public static class Application
    {
        public const string ProjectName = "MHAuthorWebsite";
        public const string WebsiteName = "Миглена Хаджипенчева"; // TODO Use this for Layout and branding

        public const string Currency = "BGN";
    }

    public static class Pages
    {
        public static class Admin
        {
            public const string Dashboard = nameof(Dashboard);
            public const string AllProducts = nameof(AllProducts);
            public const string AddProduct = nameof(AddProduct);
            public const string EditProduct = nameof(EditProduct);
            public const string AddProductType = nameof(AddProductType);
        }
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

    public static class Econt
    {
        public const int ShopId = 8661922;
        public const string ShipmentCalcUrl = "https://delivery.econt.com/customer_info.php";
        public const string UpdateOrderEndpoint = "https://delivery.econt.com/services/OrdersService.updateOrder.json";
        public const string CreateAwbEndpoint = "https://delivery.econt.com/services/OrdersService.createAWB.json";
    }
}