namespace MHAuthorWebsite.Web.ViewModels.Order;

public class SelectedProductViewModel
{
    public string Name { get; set; } = null!;

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public string ImageUrl { get; set; } = null!;

    public decimal Weight { get; set; } = 0.5m;
}