using static MHAuthorWebsite.GCommon.AppEnvironment;

namespace MHAuthorWebsite.GCommon;

public static class ApplicationRules
{
    public static class Application
    {
        public const string ProjectName = "MHAuthorWebsite";
        public const string WebsiteName = "Миглена Хаджипенчева"; // TODO Use this for Layout and branding

        public const string Currency = "BGN";

        public const string CurrentVersion = "1.0.0";
    }

    public static class CommentImages
    {
        public const int ImageMaxWidth = 1300;
        public const int ImagePreviewMaxWidth = 250;
    }

    public static class Roles
    {
        public const string AdminRoleName = "Admin";
    }

    public static class DataCollection
    {
        public const byte UsersActivityForPeriod = 30; // Days
    }

    public static class OrderSystemEventsMessages
    {
        public const string AwaitingApproval = "Поръчката е създадена успешно и очаква да бъде одобрена!";
        public const string Accepted = "Поръчката е одобрена и се подготвя за изпращане!";
        public const string Rejected = "Поръчката е отказана и няма да бъде изпратена!";
        public const string Terminated = "Поръчката е прекратена и няма да бъде доставена!";
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
            public const string AllOrders = nameof(AllOrders);
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
        public static readonly string ImageFolder = $"{Application.ProjectName}/{EnvironmentName}/products/originals";
        public static readonly string ThumbnailFolder = $"{Application.ProjectName}/{EnvironmentName}/products/thumbnails";

        public static readonly string CommentImagesFolder = $"{Application.ProjectName}/{EnvironmentName}/comments/images";
        public static readonly string CommentImagePreviewsFolder = $"{Application.ProjectName}/{EnvironmentName}/comments/previews";
    }

    public static class Econt
    {
        public const string ShipmentCalcUrl = "https://delivery.econt.com/customer_info.php";
        public const string UpdateOrderEndpoint = "https://delivery.econt.com/services/OrdersService.updateOrder.json";
        public const string CreateAwbEndpoint = "https://delivery.econt.com/services/OrdersService.createAWB.json";
        public const string DeleteLabelEndpoint = "https://delivery.econt.com/services/OrdersService.deleteLabel.json";
        public const string GetTraceEndpoint = "https://delivery.econt.com/services/OrdersService.getTrace.json";

        public const string EcontTrackerUrl = "https://www.econt.com/services/track-shipment";
    }
}