namespace MHAuthorWebsite.Core.Dtos.Order;

public class OrderSummaryDto
{
    public ICollection<SelectedProductDto> SelectedProducts { get; set; } = new HashSet<SelectedProductDto>();

    public UserDataDto UserData { get; set; } = null!;

    public decimal Subtotal => SelectedProducts.Sum(sp => sp.TotalPrice);

    public decimal SubtotalDiscountApplied => SelectedProducts.Sum(sp => sp.TotalPriceWithDiscount ?? sp.TotalPrice);

    public decimal TotalWeight => SelectedProducts.Sum(sp => sp.TotalWeight);

    public int EcontShopId { get; set; }
}