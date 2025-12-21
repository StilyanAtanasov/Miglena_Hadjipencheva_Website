namespace MHAuthorWebsite.Core.Dtos.Cart;

public class CartDto
{
    public Guid DiscountStateId { get; set; }

    public ICollection<CartItemDto> Items { get; set; } = new HashSet<CartItemDto>();

    public decimal Total => Items.Where(i => i is { IsAvailable: true, IsDiscontinued: false, IsSelected: true }).Sum(i => i.LineTotal);
}