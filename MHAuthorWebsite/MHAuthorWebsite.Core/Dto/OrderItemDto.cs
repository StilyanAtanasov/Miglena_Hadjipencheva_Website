namespace MHAuthorWebsite.Core.Dto;

public class OrderItemDto
{
    public string Name { get; set; } = null!;

    public string? Sku { get; set; }

    public string? Url { get; set; }

    public int Count { get; set; }

    public byte HideCount { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal TotalWeight { get; set; }
}