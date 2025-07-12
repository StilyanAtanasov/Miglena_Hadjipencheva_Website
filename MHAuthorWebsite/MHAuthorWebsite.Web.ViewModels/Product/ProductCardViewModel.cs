using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Web.ViewModels.Product;

public class ProductCardViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string ShortDescription { get; set; } = null!;

    public string ProductType { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }
}