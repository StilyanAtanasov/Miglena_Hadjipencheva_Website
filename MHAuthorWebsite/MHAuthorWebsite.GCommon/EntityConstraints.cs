namespace MHAuthorWebsite.GCommon;

public static class EntityConstraints
{
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
        public const int QuantityMinValue = 1;

        public const string UnitPriceSqlType = "decimal(18, 2)";
    }

    public static class Shipment
    {
        public const string ShippingPriceSqlType = "decimal(18, 2)";

        public const int ShipmentNumberMaxLength = 100;

        public const int OrderNumberMaxLength = 50;

        public const int PhoneMinLength = 6;
        public const int PhoneMaxLength = 20;

        public const int FaceMinLength = 2;
        public const int FaceMaxLength = 150;

        public const int EmailMinLength = 5;
        public const int EmailMaxLength = 100;

        public const int CurrencyMinLength = 2;
        public const int CurrencyMaxLength = 5;

        public const int AddressMinLength = 5;
        public const int AddressMaxLength = 250;

        public const int CityMinLength = 2;
        public const int CityMaxLength = 100;

        public const int PostCodeMinLength = 3;
        public const int PostCodeMaxLength = 20;

        public const int PriorityFromMinLength = 2;
        public const int PriorityFromMaxLength = 50;

        public const int PriorityToMinLength = 2;
        public const int PriorityToMaxLength = 50;

        public const int ShipmentDescriptionMinLength = 2;
        public const int ShipmentDescriptionMaxLength = 1000;
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