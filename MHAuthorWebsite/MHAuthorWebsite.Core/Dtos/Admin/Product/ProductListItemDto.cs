namespace MHAuthorWebsite.Core.Dtos.Admin.Product;

public class ProductListItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsPublic { get; set; }

    public bool HasActiveDiscount { get; set; }

    public string ProductTypeName { get; set; } = null!;
}