using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductType;

namespace MHAuthorWebsite.Data.Models;
public class ProductType
{
    [Key]
    [Comment("Primary key")]
    public int Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    [Comment("Name of product type")]
    public string Name { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new HashSet<Product>();

    public ICollection<ProductAttributeDefinition> AttributeDefinitions { get; set; } = new HashSet<ProductAttributeDefinition>();
}