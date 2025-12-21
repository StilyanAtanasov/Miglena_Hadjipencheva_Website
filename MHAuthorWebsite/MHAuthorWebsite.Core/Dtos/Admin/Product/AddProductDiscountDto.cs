namespace MHAuthorWebsite.Core.Dtos.Admin.Product;

public class AddProductDiscountDto
{
    public Guid ProductId { get; set; }

    public decimal NewPrice { get; set; }

    public decimal CurrentPrice { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}