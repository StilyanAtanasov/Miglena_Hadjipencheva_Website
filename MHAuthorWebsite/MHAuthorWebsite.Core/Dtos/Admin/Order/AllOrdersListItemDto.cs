using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.Admin.Order;

public class AllOrdersListItemDto
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = null!;

    public OrderStatus Status { get; set; }
}