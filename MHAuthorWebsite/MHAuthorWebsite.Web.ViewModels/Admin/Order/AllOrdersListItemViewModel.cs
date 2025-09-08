namespace MHAuthorWebsite.Web.ViewModels.Admin.Order;

public class AllOrdersListItemViewModel
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;
}