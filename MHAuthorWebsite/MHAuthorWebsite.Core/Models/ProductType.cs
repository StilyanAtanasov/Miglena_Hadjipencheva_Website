using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ProductType;

namespace MHAuthorWebsite.Core.Models;
public class ProductType
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new HashSet<Product>();

    public ICollection<ProductAttributeDefinition> AttributeDefinitions { get; set; } = new HashSet<ProductAttributeDefinition>();
}