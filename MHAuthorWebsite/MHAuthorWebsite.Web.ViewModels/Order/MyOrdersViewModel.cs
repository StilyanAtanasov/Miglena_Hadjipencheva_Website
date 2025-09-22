namespace MHAuthorWebsite.Web.ViewModels.Order;

public class MyOrdersViewModel
{
    public Guid OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = null!;

    public ICollection<MyOrdersOrderProductViewModel> Products { get; set; } = null!;
}