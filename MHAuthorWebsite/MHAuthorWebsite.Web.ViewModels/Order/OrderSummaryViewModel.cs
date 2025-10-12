namespace MHAuthorWebsite.Web.ViewModels.Order;

public class OrderSummaryViewModel
{
    public ICollection<SelectedProductViewModel> SelectedProducts { get; set; } = new HashSet<SelectedProductViewModel>();

    public UserDataViewModel UserData { get; set; } = null!;

    public decimal Subtotal => SelectedProducts.Sum(sp => sp.TotalPrice);

    public decimal TotalWeight => SelectedProducts.Sum(sp => sp.TotalWeight);

    public int EcontShopId { get; set; }
}