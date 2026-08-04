namespace MHAuthorWebsite.Core.Dtos.Order;

public class SelectedProductDto
{
    public string Name { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public decimal? TotalPriceWithDiscount { get; set; }

    public int Quantity { get; set; }

    public string ImageUrl { get; set; } = null!;

    public decimal TotalWeight { get; set; }
}