namespace MHAuthorWebsite.GCommon;

public static class EntityConstraints
{
    public static class ApplicationUser
    {
        public const byte NameMinLength = 2;
        public const byte NameMaxLength = 120;

        public const byte PasswordMinLength = 6;
        public const byte PasswordMaxLength = 100;
    }

    public static class Product
    {
        public const byte NameMinLength = 2;
        public const byte NameMaxLength = 50;

        public const byte DescriptionTextMinLength = 10;
        public const ushort DescriptionTextMaxLength = 4000;

        public const byte DescriptionDeltaMinLength = 10;
        public const ushort DescriptionDeltaMaxLength = 20000;

        public const byte PriceMinValue = 0;
        public const ushort PriceMaxValue = 25_000;
        public const string PriceSqlType = "decimal(18, 2)";

        public const byte StockQuantityMinValue = 0;
        public const uint StockQuantityMaxValue = 1_000_000;

        public const byte WeightMinValue = 0;
        public const ushort WeightMaxValue = 1_000;
        public const string WeightSqlType = "decimal(18, 3)";

        public const bool IsPublicDefaultValue = false;
        public const bool IsDeletedDefaultValue = false;
    }

    public static class OrderProduct
    {
        public const byte QuantityMinValue = 1;

        public const string UnitPriceSqlType = "decimal(18, 2)";
    }

    public static class Shipment
    {
        public const string ShippingPriceSqlType = "decimal(18, 2)";

        public const byte ShipmentNumberMaxLength = 100;

        public const byte OrderNumberMaxLength = 50;

        public const byte AwbUrlMaxLength = 255;

        public const byte PhoneMinLength = 6;
        public const byte PhoneMaxLength = 20;

        public const byte FaceMinLength = 2;
        public const byte FaceMaxLength = 150;

        public const byte EmailMinLength = 5;
        public const byte EmailMaxLength = 100;

        public const byte CurrencyMinLength = 2;
        public const byte CurrencyMaxLength = 5;

        public const byte AddressMinLength = 5;
        public const byte AddressMaxLength = 250;

        public const byte CityMinLength = 2;
        public const byte CityMaxLength = 100;

        public const byte PostCodeMinLength = 3;
        public const byte PostCodeMaxLength = 20;

        public const byte PriorityFromMinLength = 2;
        public const byte PriorityFromMaxLength = 50;

        public const byte PriorityToMinLength = 2;
        public const byte PriorityToMaxLength = 50;

        public const byte ShipmentDescriptionMinLength = 2;
        public const ushort ShipmentDescriptionMaxLength = 1000;
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

    public static class CartItem
    {
        public const bool IsSelectedDefaultValue = true;
    }
}