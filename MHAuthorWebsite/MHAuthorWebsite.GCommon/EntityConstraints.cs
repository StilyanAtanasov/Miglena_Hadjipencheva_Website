namespace MHAuthorWebsite.GCommon;

public static class EntityConstraints
{
    public static class Product
    {
        public const byte NameMinLength = 2;
        public const byte NameMaxLength = 50;
        public const byte DescriptionMinLength = 15;
        public const ushort DescriptionMaxLength = 4000;

        public const byte PriceMinValue = 0;
        public const ushort PriceMaxValue = 25_000;

        public const byte StockQuantityMinValue = 0;
        public const uint StockQuantityMaxValue = 1_000_000;

        public const bool IsPublicDefaultValue = false;
        public const bool IsDeletedDefaultValue = false;

        public const string PriceSqlType = "decimal(18, 2)";
    }

    public static class Order
    {
        public const string PriceSqlType = "decimal(18, 2)";
    }

    public static class Image
    {
        public const byte UrlMaxLength = 255;
        public const byte AltTextMaxLength = 150;
    }

    public static class ProductAttribute
    {
        public const byte KeyMinLength = 2;
        public const byte KeyMaxLength = 50;
        public const ushort ValueMaxLength = 2000;
    }

    public static class ProductAttributeDefinition
    {
        public const byte KeyMinLength = 2;
        public const byte KeyMaxLength = 50;
        public const byte LabelMinLength = 2;
        public const byte LabelMaxLength = 100;
    }

    public static class ProductType
    {
        public const byte NameMinLength = 2;
        public const byte NameMaxLength = 50;
    }

    public static class Comment
    {
        public const byte TextMinLength = 2;
        public const ushort TextMaxLength = 2000;
    }
}