namespace MHAuthorWebsite.GCommon;

public static class ApplicationRules
{
    public static class Pagination
    {
        public const byte PageSize = 10;
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
}