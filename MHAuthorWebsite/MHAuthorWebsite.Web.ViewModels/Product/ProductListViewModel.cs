namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductListViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string ProductTypeName { get; set; } = null!;
}