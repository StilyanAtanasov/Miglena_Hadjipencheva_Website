namespace MHAuthorWebsite.Core.Dto;

public class OrderItemDto
{
    public string Name { get; set; } = null!;

    public int Count { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal TotalWeight { get; set; }
}