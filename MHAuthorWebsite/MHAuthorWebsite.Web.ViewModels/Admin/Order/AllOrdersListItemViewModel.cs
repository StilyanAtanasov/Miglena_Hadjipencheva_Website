using MHAuthorWebsite.Data.Models.Enums;

namespace MHAuthorWebsite.Web.ViewModels.Admin.Order;

public class AllOrdersListItemViewModel
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = null!;

    public OrderStatus Status { get; set; }
}